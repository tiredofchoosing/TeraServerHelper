using System.Net;

namespace NetworkSniffer
{
    public class TcpConnection
    {
        private readonly int maxBufferedPackets = 50;
        private readonly int maxBufferedPacketLength = 500;
        private readonly SortedDictionary<long, byte[]> _bufferedPackets = new();
        internal readonly ConnectionId ConnectionId;

        public readonly IPEndPoint Destination;
        public readonly IPEndPoint Source;
        public Action RemoveCallback;

        public event Action<TcpConnection, byte[], int>? DataReceived;

        public long BytesReceived { get; private set; }
        public uint InitialSequenceNumber { get; }
        public uint CurrentSequenceNumber => unchecked((uint)(InitialSequenceNumber + 1 + BytesReceived));
        public uint NextSequenceNumber { get; private set; }
        public bool HasSubscribers => DataReceived != null;
        internal string BufferedPacketDescription => 
            string.Join(", ", _bufferedPackets.OrderBy(x => x.Key).Select(x => $"{x.Key} + {x.Value.Length}"));
        
        internal TcpConnection(ConnectionId connectionId, uint sequenceNumber, Action<TcpConnection> removeCallback)
        {
            ConnectionId = connectionId;
            Source = connectionId.Source;
            Destination = connectionId.Destination;
            InitialSequenceNumber = sequenceNumber;
            RemoveCallback = () => removeCallback(this);
        }

        public long SequenceNumberToBytesReceived(uint sequenceNumber)
        {
            var offsetToCurrent = unchecked((int)(sequenceNumber - CurrentSequenceNumber));
            return BytesReceived + offsetToCurrent;
        }

        internal void OnDataReceived(byte[] data, int needToSkip)
        {
            DataReceived?.Invoke(this, data, needToSkip);
        }

        internal void HandleTcpReceived(uint sequenceNumber, byte[] data)
        {
            var dataPosition = SequenceNumberToBytesReceived(sequenceNumber);
            long needToSkip = 0;
            NextSequenceNumber = (uint)(sequenceNumber + data.Length);
            if (dataPosition == BytesReceived)
            {
                OnDataReceived(data, (int)needToSkip);
                BytesReceived += data.Length;
            }
            else
            {
                //if (!_bufferedPackets.ContainsKey(dataPosition) ||
                //    _bufferedPackets[dataPosition].Length < data.Length)
                //{
                _bufferedPackets[dataPosition] = data;
                //}
            }

            if (_bufferedPackets.Count > maxBufferedPackets)
            {
                while (_bufferedPackets.Values.First().Length >= maxBufferedPacketLength)
                {
                    _bufferedPackets.Remove(_bufferedPackets.Keys.First());
                    if (_bufferedPackets.Count == 0)
                        return;
                }
                //we don't know, whether large packet is continuation of previous message or not -
                //so skip until new short message.
                if (BytesReceived + maxBufferedPackets <= _bufferedPackets.Keys.First())
                {
                    _bufferedPackets.Remove(_bufferedPackets.Keys.First());
                }
                //and even after skipping long fragments we don't know,
                //whether small fragment after big is a new short message or a big message tail -
                //skip small one too.
                needToSkip = _bufferedPackets.Keys.First() - BytesReceived;
                BytesReceived = _bufferedPackets.Keys.First();
            }

            long firstBufferedPosition;
            while (_bufferedPackets.Any() && (firstBufferedPosition = _bufferedPackets.Keys.First()) <= BytesReceived)
            {
                var dataArray = _bufferedPackets[firstBufferedPosition];
                _bufferedPackets.Remove(firstBufferedPosition);

                var alreadyReceivedBytes = BytesReceived - firstBufferedPosition;
                //Debug.Assert(alreadyReceivedBytes >= 0);

                if (alreadyReceivedBytes > dataArray.Length)
                    continue;

                var count = dataArray.Length - alreadyReceivedBytes;
                OnDataReceived(dataArray.Skip((int)alreadyReceivedBytes).Take((int)count).ToArray(), (int)needToSkip);
                BytesReceived += count;
                needToSkip = 0;
            }
        }

        public override string ToString()
        {
            return $"{Source} -> {Destination}";
        }
    }
}