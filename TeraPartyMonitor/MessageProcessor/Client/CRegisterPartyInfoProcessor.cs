using NLog;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class CRegisterPartyInfoProcessor : TeraMessageProcessor
    {
        public CRegisterPartyInfoProcessor(ParsedMessage message, Client client, TeraDataPools dataPools, ILogger logger) 
            : base(message, client, dataPools, logger) { }

        public override void Process()
        {
            if (Message is CRegisterPartyInfoMessage m)
            {
                var player = Client.CurrentPlayer;
                var party = DataPools.GetOrCreatePartyByPlayer(player);

                var oldPartyInfo = DataPools.GetPartyInfoByParty(party);
                var newPartyInfo = new PartyInfo(party, m.Message, m.IsRaid);

                if (oldPartyInfo != null)
                {
                    DataPools.Replace(oldPartyInfo, newPartyInfo);
                    Logger.Debug($"{Client}|Modified PartyInfo: {oldPartyInfo} -> {newPartyInfo}.");
                }
                else
                {
                    DataPools.Add(newPartyInfo);
                    Logger.Debug($"{Client}|Added PartyInfo: {newPartyInfo}.");
                }
            }
        }
    }
}