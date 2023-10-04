using System.ComponentModel;
using System.IO;
using System.Numerics;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class SDelInterPartyMatchPoolProcessor : TeraMessageProcessor
    {
        public SDelInterPartyMatchPoolProcessor(ParsedMessage message, Client client, TeraDataPools dataPools) : base(message, client, dataPools) { }

        public override void Process()
        {
            if (Message is SDelInterPartyMatchPoolMessage m)
            {
                var player = Client.CurrentPlayer;
                PartyMatching? partyMatching1, partyMatching2 = null;

                switch (m.MatchingType)
                {
                    case MatchingTypes.Dungeon:
                    case MatchingTypes.Battleground:
                        partyMatching1 = DataPools.GetPartyMatchingByPlayer(player, m.MatchingType);
                        break;
                    case MatchingTypes.All:
                        partyMatching1 = DataPools.GetPartyMatchingByPlayer(player, MatchingTypes.Dungeon);
                        partyMatching2 = DataPools.GetPartyMatchingByPlayer(player, MatchingTypes.Battleground);
                        break;
                    default:
                        throw new MatchingTypesInvalidEnumArgumentException(m.MatchingType);
                }

                TryRemove(partyMatching1, player);
                TryRemove(partyMatching2, player);
            }
        }

        private void TryRemove(PartyMatching partyMatching, Player player)
        {
            if (partyMatching == null)
                return;

            if (!partyMatching.MatchingProfiles.First().LinkedPlayer.Equals(player))
                return;

            DataPools.Remove(partyMatching);
        }
    }
}