using System.Collections.Concurrent;
using PacketDotNet;

namespace NetworkSniffer
{
    public class TcpSniffer
    {
        private readonly ConcurrentDictionary<ConnectionId, (TcpConnection, object)> _connections = new();
        //private readonly object _lock = new object();
        private readonly IpSniffer _ipSniffer;

        public event Action<TcpConnection>? NewConnection;
        public event Action<TcpConnection>? EndConnection;

        public TcpSniffer(IpSniffer ipSniffer)
        {
            _ipSniffer = ipSniffer;
            _ipSniffer.PacketReceived += Receive;
        }

        protected void OnNewConnection(TcpConnection connection)
        {
            NewConnection?.Invoke(connection);
        }

        protected void OnEndConnection(TcpConnection connection)
        {
            EndConnection?.Invoke(connection);
        }

        internal void RemoveConnection(TcpConnection connection)
        {
            _connections.TryRemove(connection.ConnectionId, out var temp);
        }

        private void Receive(IPv4Packet ipData)
        {
            if (ipData.PayloadPacket is not TcpPacket tcpPacket || tcpPacket.DataOffset * 4 > ipData.PayloadLength)
                return;

            // Ack-only packets aren't interesting
            var isAckOnly = tcpPacket.Flags == TcpFields.TCPAckMask;
            if (isAckOnly)
                return;

            var isSync = tcpPacket.Synchronize;
            var isFinOrRst = tcpPacket.Finished || tcpPacket.Reset;
            var connectionId = new ConnectionId(ipData.SourceAddress, tcpPacket.SourcePort, ipData.DestinationAddress, tcpPacket.DestinationPort);

            if (isSync)
            {
                return;
                var connection = new TcpConnection(connectionId, tcpPacket.SequenceNumber, RemoveConnection);
                OnNewConnection(connection);

                if (!connection.HasSubscribers)
                    return;

                _connections[connectionId] = (connection, new object());
            }
            else
            {
                return;
                if (!_connections.TryGetValue(connectionId, out var connectionData))
                    return;

                (TcpConnection? connection, object locker) = connectionData;

                if (isFinOrRst)
                {
                    OnEndConnection(connection);

                    if (_connections.TryGetValue(connection.ConnectionId.Reverse, out var reverse))
                    {
                        OnEndConnection(reverse.Item1);
                    }

                    Task.Run(() => { Task.Delay(1000); GC.Collect(); });
                    return;
                }

                byte[] payload;
                try
                {
                    payload = tcpPacket.PayloadData;
                }
                catch
                {
                    return;
                }

                if (payload == null || payload.Length == 0)
                    return;

                lock (locker)
                {
                    connection.HandleTcpReceived(tcpPacket.SequenceNumber, payload);
                }
            }
        }
    }
}