using NLog;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class SLoginProcessor : TeraMessageProcessor
    {
        public SLoginProcessor(ParsedMessage message, Client client, TeraDataPools dataPools, NLog.ILogger logger)
            : base(message, client, dataPools, logger) { }

        public override void Process()
        {
            if (Message is SLoginMessage m)
            {
                var player = DataPools.GetPlayerByName(m.Name);
                if (player != null)
                {
                    Logger.Error($"{Client}|Player {player} is already logged in!");
                    DataPools.Remove(player);

                    var client = DataPools.GetClientByPlayer(player);
                    if (client != null && client != Client)
                    {
                        client.CurrentPlayer = null;
                        DataPools.Remove(client);
                    }
                }

                player = new Player(m.EntityId, m.PlayerId, m.Name, m.Level, m.Class, m.Race, m.Gender);
                DataPools.Add(player);
                Client.CurrentPlayer = player;
                Logger.Debug($"{Client}|Player login: {player}.");
            }
        }
    }
}