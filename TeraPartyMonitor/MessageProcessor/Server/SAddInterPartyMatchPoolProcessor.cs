using System.ComponentModel;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class SAddInterPartyMatchPoolProcessor : TeraMessageProcessor
    {
        public SAddInterPartyMatchPoolProcessor(ParsedMessage message, Client client, TeraDataPools dataPools) : base(message, client, dataPools) { }

        public override void Process()
        {
            if (Message is SAddInterPartyMatchPoolMessage m)
            {
                var player = Client.CurrentPlayer;

                if (m.Profiles.First().Name != player.Name)
                    return;

                //var party = DataPools.GetOrCreatePartyByPlayer(player);

                //if (player != party.Leader)
                //    return;

                //if (party.Players.Count != m.Profiles.Count)
                //    throw new Exception("Players count from Party struct is not equal to received MatchingPlayer count");

                //bool check = party.Players.All(p1 => m.Profiles.Any(p2 => p1.Name == p2.Name));
                //if (!check)
                //    throw new Exception("Players from Party struct is not equal to received MatchingPlayer");

                foreach (var profile in m.Profiles)
                {
                    profile.LinkedPlayer = DataPools.GetPlayerByName(profile.Name);

                    //var profile = m.Profiles.Single(p2 => p.Name == p2.Name);
                    //switch (m.MatchingType)
                    //{
                    //    case MatchingTypes.Dungeon:
                    //        p.DungeonMatchingProfile = profile;
                    //        break;
                    //    case MatchingTypes.Battleground:
                    //        p.BattlegroundMatchingProfile = profile;
                    //        break;
                    //    default:
                    //        throw new MatchingTypesInvalidEnumArgumentException(m.MatchingType);
                    //}
                }

                var partyMatching = new PartyMatching(m.Profiles, m.Instances, m.MatchingType);
                DataPools.PartyMatchingCollection.Add(partyMatching);
            }
        }
    }
}