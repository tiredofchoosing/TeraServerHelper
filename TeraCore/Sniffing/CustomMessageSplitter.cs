using System.Net;

namespace TeraCore.Sniffing
{
    public class CustomMessageSplitter : MessageSplitter
    {
        IPEndPoint _client;
        public CustomMessageSplitter(IPEndPoint client) : base()
        {
            _client = client;
        }

        public event Action<Message, IPEndPoint> MessageClientReceived;

        protected override void OnMessageReceived(Message message)
        {
            base.OnMessageReceived(message);
            MessageClientReceived?.Invoke(message, _client);
        }
    }
}
