using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class SLoginProcessor : TeraMessageProcessor
    {
        public SLoginProcessor(ParsedMessage message, Client client, TeraDataPools dataPools) : base(message, client, dataPools) { }

        public override void Process()
        {
            if (Message is SLoginMessage m)
            {
                var player = new Player(m.PlayerId, m.Name, m.Level, m.Class);
                DataPools.PlayerCollection.Add(player);
                Client.CurrentPlayer = player;
                //Console.WriteLine($"{Client.EndPoint.Address}:{Client.EndPoint.Port}: New player has log on: {player}");
            }
        }
    }
}