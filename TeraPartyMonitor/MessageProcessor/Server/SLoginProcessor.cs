using NLog;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class SLoginProcessor : TeraMessageProcessor
    {
        public SLoginProcessor(ParsedMessage message, Client client, TeraDataPools dataPools, ILogger logger)
            : base(message, client, dataPools, logger) { }

        public override void Process()
        {
            if (Message is SLoginMessage m)
            {
                var player = new Player(m.PlayerId, m.Name, m.Level, m.Class);
                DataPools.Add(player);
                Client.CurrentPlayer = player;
                Logger.Debug($"{Client}|Player login: {player}.");
            }
        }
    }
}