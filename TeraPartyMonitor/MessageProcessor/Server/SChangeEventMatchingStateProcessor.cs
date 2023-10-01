using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class SChangeEventMatchingStateProcessor : TeraMessageProcessor
    {
        public SChangeEventMatchingStateProcessor(ParsedMessage message, Client client, TeraDataPools dataPools) : base(message, client, dataPools) { }

        public override void Process()
        {
            if (Message is S_CHANGE_EVENT_MATCHING_STATE m)
            {
                var player = Client.CurrentPlayer;
                var party = DataPools.GetOrCreatePartyByPlayer(player);
                var partyMatching = DataPools.GetPartyMatchingByParty(party);

                if (partyMatching == null && m.Searching)
                {
                    var dungeons = new List<Dungeon>(m.MatchingEvents.Count);
                    foreach (var d in m.MatchingEvents)
                    {
                        dungeons.Add(new Dungeon(d));
                    }
                    partyMatching = new PartyMatching(party, dungeons);
                    DataPools.PartyMatchingCollection.Add(partyMatching);
                }
                else if (partyMatching != null && !m.Searching)
                {
                    DataPools.PartyMatchingCollection.Remove(partyMatching);
                }
                else
                {
                    Console.WriteLine("Invalid Party matching process");
                }
                return;
            }
        }
    }
}