using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class SReturnToLobbyProcessor : TeraMessageProcessor
    {
        public SReturnToLobbyProcessor(ParsedMessage message, Client client, TeraDataPools dataPools) : base(message, client, dataPools) { }

        public override void Process()
        {
            if (Message is SReturnToLobbyMessage)
            {
                if (Client.CurrentPlayer != null)
                {
                    DataPools.Remove(Client.CurrentPlayer);
                    Client.CurrentPlayer = null;
                }
                return;
            }
        }
    }
}