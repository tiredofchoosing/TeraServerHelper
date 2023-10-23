using System.Net;

namespace TeraCore.Sniffing
{
    public class CustomMessageSplitter : MessageSplitter
    {
        readonly IPEndPoint _client;

        public event Action<Message, IPEndPoint>? MessageClientReceived;

        public CustomMessageSplitter(IPEndPoint client) : base()
        {
            _client = client;
        }

        protected override void OnMessageReceived(Message message)
        {
            base.OnMessageReceived(message);
            MessageClientReceived?.Invoke(message, _client);
        }
    }
}
