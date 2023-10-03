using System.ComponentModel;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class SModifyInterPartyMatchPoolProcessor : TeraMessageProcessor
    {
        public SModifyInterPartyMatchPoolProcessor(ParsedMessage message, Client client, TeraDataPools dataPools) : base(message, client, dataPools) { }

        public override void Process()
        {
            if (Message is SModifyInterPartyMatchPoolMessage m)
            {
                var player = Client.CurrentPlayer;
                var partyMatching1 = DataPools.GetPartyMatchingByPlayer(player, MatchingTypes.Dungeon);
                var partyMatching2 = DataPools.GetPartyMatchingByPlayer(player, MatchingTypes.Battleground);

                TryModify(partyMatching1, player, m.Modifiers);
                TryModify(partyMatching2, player, m.Modifiers);
            }
        }

        private void TryModify(PartyMatching partyMatching, Player player, IList<(string, bool)> modifiers)
        {
            if (partyMatching == null)
                return;

            if (!partyMatching.MatchingProfiles.First().LinkedPlayer.Equals(player))
                return;

            var profiles = new List<MatchingProfile>();

            foreach ((var name, var isLeader) in modifiers)
            {
                var profile = partyMatching.MatchingProfiles.Single(p => p.Name.Equals(name));
                var role = profile.Role;
                var linkedPlayer = profile.LinkedPlayer;

                profiles.Add(new MatchingProfile(name, isLeader, role));
            }
            partyMatching.MatchingProfiles = profiles;
        }
    }
}