using TeraCore.Game.Structures;

namespace TeraPartyMonitor.DataSender.Models
{
    [Serializable]
    public abstract class PartyMatchingModel
    {
        public int MatchingId { get; set; }
        public IList<MatchingPartyModel> Parties { get; set; }

        public PartyMatchingModel(int id, IEnumerable<IList<MatchingProfile>> profiles)
        {
            MatchingId = id;
            Parties = new List<MatchingPartyModel>();
            int i = 1;
            foreach (var p in profiles)
            {
                Parties.Add(new MatchingPartyModel()
                {
                    PartyId = i++,
                    Players = p
                });
            }
        }
    }
}