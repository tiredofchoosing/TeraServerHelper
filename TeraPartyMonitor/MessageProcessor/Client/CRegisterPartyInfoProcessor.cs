using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class CRegisterPartyInfoProcessor : TeraMessageProcessor
    {
        public CRegisterPartyInfoProcessor(ParsedMessage message, Client client, TeraDataPools dataPools) : base(message, client, dataPools) { }

        public override void Process()
        {
            if (Message is CRegisterPartyInfoMessage m)
            {
                var player = Client.CurrentPlayer;
                var party = DataPools.GetOrCreatePartyByPlayer(player);

                var partyInfo = DataPools.GetPartyInfoByParty(party);
                if (partyInfo != null)
                {
                    DataPools.Remove(partyInfo);
                }
                partyInfo = new PartyInfo(party, m.Message, m.IsRaid);
                DataPools.Add(partyInfo);
                return;
            }
        }
    }
}