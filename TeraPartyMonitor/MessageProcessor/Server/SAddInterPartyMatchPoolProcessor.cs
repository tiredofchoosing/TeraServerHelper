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

                foreach (var profile in m.Profiles)
                {
                    profile.LinkedPlayer = DataPools.GetPlayerByName(profile.Name);
                }

                var partyMatching = new PartyMatching(m.Profiles, m.Instances, m.MatchingType);
                DataPools.Add(partyMatching);
            }
        }
    }
}