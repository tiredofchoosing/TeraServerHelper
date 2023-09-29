// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NetworkSniffer;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly HashSet<TcpConnection> _isNew = new HashSet<TcpConnection>();
        private readonly Dictionary<IPEndPoint, CustomConnectionDecrypter> _decrypters = new();
        private readonly Dictionary<IPEndPoint, CustomMessageSplitter> _messageSplitters = new();
        private readonly Dictionary<IPEndPoint, Client> _clients = new();
        private readonly IpSnifferWinPcap _ipSniffer;

        public event Action<string> Warning;
        public event Action<Client> NewClientConnection;
        public event Action<Client> EndClientConnection;
        public event Action<Message, Client> MessageClientReceived;

        public CustomTeraSniffer(CustomServer server)
        {
            _server = server;
            var filter = $"tcp and host {server.Ip} and port {server.Port}";

            _ipSniffer = new IpSnifferWinPcap(filter);
            _ipSniffer.Warning += OnWarning;
            var tcpSniffer = new TcpSniffer(_ipSniffer);
            tcpSniffer.NewConnection += HandleNewConnection;
            tcpSniffer.EndConnection += HandleEndConnection;
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
            var client = _clients[clientEndPoint];
            MessageClientReceived?.Invoke(message, client);
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

        protected virtual void OnWarning(string obj)
        {
            Warning?.Invoke(obj);
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

                bool isInteresting = _server.EndPoint.Equals(src); ;// || _server.EndPoint.Equals(dst);
                if (!isInteresting)
                    return;

                var clientEndPoint = GetClientEndPoint(connection);
                var client = _clients[clientEndPoint];
                _clients.Remove(clientEndPoint);

                connection.DataReceived -= HandleTcpDataReceived;

                if (_decrypters.ContainsKey(clientEndPoint))
                {
                    _decrypters[clientEndPoint].CustomClientToServerDecrypted -= HandleClientToServerDecrypted;
                    _decrypters[clientEndPoint].CustomServerToClientDecrypted -= HandleServerToClientDecrypted;
                    _decrypters.Remove(clientEndPoint);
                }
                if (_messageSplitters.ContainsKey(clientEndPoint))
                {
                    _messageSplitters[clientEndPoint].MessageClientReceived -= HandleMessageClientReceived;
                    _messageSplitters.Remove(clientEndPoint);
                }

                OnEndClientConnection(client);
            }
        }

        // called from the tcp sniffer, so it needs to lock
        void HandleTcpDataReceived(TcpConnection connection, ArraySegment<byte> data)
        {
            lock (_eventLock)
            {
                if (data.Count == 0)
                    return;

                var clientEndPoint = GetClientEndPoint(connection);

                if (_isNew.Remove(connection))
                {
                    if (_server.EndPoint.Equals(connection.Source) &&
                        data.Array.Skip(data.Offset).Take(4).SequenceEqual(new byte[] { 1, 0, 0, 0 }))
                    {
                        var client = new Client(clientEndPoint);
                        _clients.Add(clientEndPoint, client);

                        var decrypter = new CustomConnectionDecrypter(clientEndPoint);
                        decrypter.CustomClientToServerDecrypted += HandleClientToServerDecrypted;
                        decrypter.CustomServerToClientDecrypted += HandleServerToClientDecrypted;
                        _decrypters.Add(clientEndPoint, decrypter);

                        var messageSplitter = new CustomMessageSplitter(clientEndPoint);
                        messageSplitter.MessageClientReceived += HandleMessageClientReceived;
                        _messageSplitters.Add(clientEndPoint, messageSplitter);

                        OnNewClientConnection(client);
                    }
                }

                if (!_clients.ContainsKey(clientEndPoint))
                    return;

                var dataArray = data.Array.Skip(data.Offset).Take(data.Count).ToArray();
                if (connection.Destination.Equals(_server.EndPoint))
                    _decrypters[clientEndPoint].ClientToServer(dataArray);
                else
                    _decrypters[clientEndPoint].ServerToClient(dataArray);
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
            if (!_messageSplitters.ContainsKey(client))
                return;

            _messageSplitters[client].ServerToClient(DateTime.UtcNow, data);
        }

        // called indirectly from HandleTcpDataReceived, so the current thread already holds the lock
        void HandleClientToServerDecrypted(IPEndPoint client, byte[] data)
        {
            if (!_messageSplitters.ContainsKey(client))
                return;

            _messageSplitters[client].ClientToServer(DateTime.UtcNow, data);
        }

        private IPEndPoint GetClientEndPoint(TcpConnection connection)
        {
            return _server.EndPoint.Equals(connection.Source) ? connection.Destination : connection.Source;
        }
    }
}
