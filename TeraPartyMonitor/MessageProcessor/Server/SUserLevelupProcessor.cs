using NLog;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class SUserLevelupProcessor : TeraMessageProcessor
    {
        public SUserLevelupProcessor(ParsedMessage message, Client client, TeraDataPools dataPools, NLog.ILogger logger)
            : base(message, client, dataPools, logger) { }

        public override void Process()
        {
            if (Message is SUserLevelupMessage m)
            {
                Client.CurrentPlayer.Level = m.Level;
                Logger.Debug($"{Client}|Player level up: {Client.CurrentPlayer}.");
            }
        }
    }
}