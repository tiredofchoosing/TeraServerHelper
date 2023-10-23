using NetworkSniffer;
using System.Net;
using TeraCore.Game;
using TeraCore.Sniffing;

namespace TeraSniffing
{
    public class CustomTeraSniffer : ICustomTeraSniffer
    {
        // Only take this lock in callbacks from tcp sniffing, not in code that can be called by the user.
        // Otherwise this could cause a deadlock if the user calls such a method from a callback that already holds a lock
        private readonly object _eventLock = new object();

        private readonly CustomServer _server;
        private readonly TcpSniffer _tcpSniffer;
        private readonly IpSnifferWinPcap _ipSniffer;
        private readonly HashSet<TcpConnection> _isNew = new();
        private readonly Dictionary<IPEndPoint, ClientData> _clientsData = new();

        public event Action<string>? Warning;
        public event Action<Client>? NewClientConnection;
        public event Action<Client>? EndClientConnection;
        public event Action<Message, Client>? MessageClientReceived;

        public CustomTeraSniffer(CustomServer server, IEnumerable<string>? pcapDeviceFilters = null)
        {
            _server = server;
            var filter = $"tcp and host {server.EndPoint.Address} and port {server.EndPoint.Port}";

            _ipSniffer = new IpSnifferWinPcap(filter, pcapDeviceFilters);
            _ipSniffer.Warning += OnWarning;
            _tcpSniffer = new TcpSniffer(_ipSniffer);
            _tcpSniffer.NewConnection += HandleNewConnection;
            _tcpSniffer.EndConnection += HandleEndConnection;
        }

        protected virtual void OnNewClientConnection(Client client)
        {
            NewClientConnection?.Invoke(client);
        }

        protected virtual void OnEndClientConnection(Client client)
        {
            EndClientConnection?.Invoke(client);
        }

        protected virtual void OnMessageClientReceived(Message message, IPEndPoint clientEndPoint)
        {
            var clientData = _clientsData[clientEndPoint];
            MessageClientReceived?.Invoke(message, clientData.Client);
        }

        // IpSniffer has its own locking, so we need no lock here.
        public bool Enabled
        {
            get => _ipSniffer.Enabled;
            set => _ipSniffer.Enabled = value;
        }

        public IEnumerable<string> SnifferStatus()
        {
            return _ipSniffer.Status();
        }

        protected virtual void OnWarning(string message)
        {
            Warning?.Invoke(message);
        }

        // called from the tcp sniffer, so it needs to lock
        void HandleNewConnection(TcpConnection connection)
        {
            lock (_eventLock)
            {
                var dst = connection.Destination;
                var src = connection.Source;

                bool isInteresting = _server.EndPoint.Equals(dst) || _server.EndPoint.Equals(src);
                if (!isInteresting)
                    return;

                _isNew.Add(connection);
                connection.DataReceived += HandleTcpDataReceived;
            }
        }

        void HandleEndConnection(TcpConnection connection)
        {
            lock (_eventLock)
            {
                var dst = connection.Destination;
                var src = connection.Source;

                if (_server.EndPoint.Equals(src) || _server.EndPoint.Equals(dst))
                    connection.DataReceived -= HandleTcpDataReceived;

                var clientEndPoint = GetClientEndPoint(connection);
                if (_clientsData.TryGetValue(clientEndPoint, out var clientData))
                {
                    _clientsData.Remove(clientEndPoint);

                    if (clientData.ConnectionDecrypter != null)
                    {
                        clientData.ConnectionDecrypter.CustomClientToServerDecrypted -= HandleClientToServerDecrypted;
                        clientData.ConnectionDecrypter.CustomServerToClientDecrypted -= HandleServerToClientDecrypted;
                    }
                    if (clientData.MessageSplitter != null)
                    {
                        clientData.MessageSplitter.MessageClientReceived -= HandleMessageClientReceived;
                    }
                    clientData.ServerToClient.RemoveCallback();
                    OnEndClientConnection(clientData.Client);
                    clientData.Dispose();
                }
                else
                    connection.RemoveCallback();
            }
        }

        // called from the tcp sniffer, so it needs to lock
        void HandleTcpDataReceived(TcpConnection connection, byte[] data, int needToSkip)
        {
            lock (_eventLock)
            {
                var clientEndPoint = GetClientEndPoint(connection);

                if (data.Length == 0)
                {
                    var exist = _clientsData.TryGetValue(clientEndPoint, out var clientData);
                    if (needToSkip == 0 || !exist)
                        return;

                    clientData.ConnectionDecrypter.Skip(connection == clientData.ClientToServer ? MessageDirection.ClientToServer : MessageDirection.ServerToClient, needToSkip);
                    return;
                }

                if (_isNew.Remove(connection))
                {
                    if (_server.EndPoint.Equals(connection.Source) &&
                        data.Take(4).SequenceEqual(new byte[] { 1, 0, 0, 0 }))
                    {
                        var client = new Client(clientEndPoint);

                        var decrypter = new CustomConnectionDecrypter(clientEndPoint);
                        decrypter.CustomClientToServerDecrypted += HandleClientToServerDecrypted;
                        decrypter.CustomServerToClientDecrypted += HandleServerToClientDecrypted;

                        var messageSplitter = new CustomMessageSplitter(clientEndPoint);
                        messageSplitter.MessageClientReceived += HandleMessageClientReceived;

                        var clientData = new ClientData()
                        {
                            Client = client,
                            ConnectionDecrypter = decrypter,
                            MessageSplitter = messageSplitter,
                            ServerToClient = connection
                        };
                        _clientsData.Add(clientEndPoint, clientData);
                    }
                    if (_clientsData.TryGetValue(clientEndPoint, out var clientData1) &&
                        clientData1.ClientToServer == null && clientData1.ServerToClient != connection)
                    {
                        clientData1.ClientToServer = connection;
                        OnNewClientConnection(clientData1.Client);
                    }
                    //if received more bytes but still not recognized - not interesting.
                    //if (connection.BytesReceived > 0x10000)
                    //{
                    //    connection.DataReceived -= HandleTcpDataReceived;
                    //    connection.RemoveCallback();
                    //}
                }

                if (!_clientsData.ContainsKey(clientEndPoint))
                    return;

                var dec = _clientsData[clientEndPoint].ConnectionDecrypter;
                if (connection.Destination.Equals(_server.EndPoint))
                    dec.ClientToServer(data, needToSkip);
                else
                    dec.ServerToClient(data, needToSkip);
            }
        }

        // called indirectly from HandleTcpDataReceived, so the current thread already holds the lock
        private void HandleMessageClientReceived(Message message, IPEndPoint client)
        {
            OnMessageClientReceived(message, client);
        }

        // called indirectly from HandleTcpDataReceived, so the current thread already holds the lock
        void HandleServerToClientDecrypted(IPEndPoint client, byte[] data)
        {
            if (!_clientsData.ContainsKey(client))
                return;

            var splitter = _clientsData[client].MessageSplitter;
            splitter.ServerToClient(DateTime.UtcNow, data);
        }

        // called indirectly from HandleTcpDataReceived, so the current thread already holds the lock
        void HandleClientToServerDecrypted(IPEndPoint client, byte[] data)
        {
            if (!_clientsData.ContainsKey(client))
                return;

            var splitter = _clientsData[client].MessageSplitter;
            splitter.ClientToServer(DateTime.UtcNow, data);
        }

        private IPEndPoint GetClientEndPoint(TcpConnection connection)
        {
            return _server.EndPoint.Equals(connection.Source) ? connection.Destination : connection.Source;
        }
    }

    class ClientData : IDisposable
    {
        public Client Client { get; init; }
        public CustomConnectionDecrypter ConnectionDecrypter { get; init; }
        public CustomMessageSplitter MessageSplitter { get; init; }
        public TcpConnection ServerToClient { get; init; }
        public TcpConnection? ClientToServer { get; set; }

        public void Dispose()
        {
            MessageSplitter.Dispose();
            ConnectionDecrypter.Dispose();
        }
    }
}
