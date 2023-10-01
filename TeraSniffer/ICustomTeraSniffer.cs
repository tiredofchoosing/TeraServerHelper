using TeraCore.Game;
using TeraCore.Sniffing;

namespace TeraSniffing
{
    public interface ICustomTeraSniffer
    {
        bool Enabled { get; set; }
        event Action<Client> NewClientConnection;
        event Action<Client> EndClientConnection;
        event Action<Message, Client> MessageClientReceived;
    }
}