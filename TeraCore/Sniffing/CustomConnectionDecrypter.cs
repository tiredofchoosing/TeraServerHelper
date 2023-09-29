using NetworkSniffer;
using System.Net;

namespace TeraCore.Sniffing
{
    public class CustomConnectionDecrypter : ConnectionDecrypter
    {
        private readonly IPEndPoint _client;

        public event Action<IPEndPoint, byte[]> CustomClientToServerDecrypted;
        public event Action<IPEndPoint, byte[]> CustomServerToClientDecrypted;

        public CustomConnectionDecrypter(IPEndPoint client, string region = "Unknown") : base()
        {
            _client = client;
        }

        protected override void OnClientToServerDecrypted(byte[] data)
        {
            base.OnClientToServerDecrypted(data);
            CustomClientToServerDecrypted?.Invoke(_client, data);
        }

        protected override void OnServerToClientDecrypted(byte[] data)
        {
            base.OnServerToClientDecrypted(data);
            CustomServerToClientDecrypted?.Invoke(_client, data);
        }
    }
}
