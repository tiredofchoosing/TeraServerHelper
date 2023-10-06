using NLog;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class SModifyInterPartyMatchPoolProcessor : TeraMessageProcessor
    {
        public SModifyInterPartyMatchPoolProcessor(ParsedMessage message, Client client, TeraDataPools dataPools, NLog.ILogger logger)
            : base(message, client, dataPools, logger) { }

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

        private void TryModify(PartyMatching oldPartyMatching, Player player, IList<(string, bool)> modifiers)
        {
            if (oldPartyMatching == null)
                return;

            if (!oldPartyMatching.MatchingProfiles.First().LinkedPlayer.Equals(player))
                return;

            var profiles = new List<MatchingProfile>();

            foreach ((var name, var isLeader) in modifiers)
            {
                var profile = oldPartyMatching.MatchingProfiles.Single(p => p.Name.Equals(name));
                var role = profile.Role;
                var linkedPlayer = profile.LinkedPlayer;

                profiles.Add(new MatchingProfile(name, isLeader, role));
            }

            var newPartyMatching = new PartyMatching(profiles, oldPartyMatching.Instances, oldPartyMatching.MatchingType);
            DataPools.Replace(oldPartyMatching, newPartyMatching);
            Logger.Debug($"{Client}|Modified PartyMatching: {oldPartyMatching} -> {newPartyMatching}.");
        }
    }
}