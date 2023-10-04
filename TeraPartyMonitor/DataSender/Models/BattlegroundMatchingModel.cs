using TeraCore.Game.Structures;

namespace TeraPartyMonitor.DataSender.Models
{
    [Serializable]
    public class BattlegroundMatchingModel : PartyMatchingModel
    {
        public Battleground Battleground { get; set; }

        public BattlegroundMatchingModel(int id, IEnumerable<IList<MatchingProfile>> profiles, Battleground battleground)
            : base(id, profiles)
        {
            Battleground = battleground;
        }
    }
}
