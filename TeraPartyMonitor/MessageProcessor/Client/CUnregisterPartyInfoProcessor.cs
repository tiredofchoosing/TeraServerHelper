using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class CUnregisterPartyInfoProcessor : TeraMessageProcessor
    {
        public CUnregisterPartyInfoProcessor(ParsedMessage message, Client client, TeraDataPools dataPools) : base(message, client, dataPools) { }

        public override void Process()
        {
            if (Message is CUnregisterPartyInfoMessage)
            {
                var player = Client.CurrentPlayer;
                var party = DataPools.GetPartyByPlayer(player);
                var partyInfo = DataPools.GetPartyInfoByParty(party);
                DataPools.PartyInfoCollection.Remove(partyInfo);
                return;
            }
        }
    }
}