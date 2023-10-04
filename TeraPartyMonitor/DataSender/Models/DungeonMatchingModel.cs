using TeraCore.Game.Structures;

namespace TeraPartyMonitor.DataSender.Models
{
    [Serializable]
    public class DungeonMatchingModel : PartyMatchingModel
    {
        public Dungeon Dungeon { get; set; }

        public DungeonMatchingModel(int id, IEnumerable<IList<MatchingProfile>> profiles, Dungeon dungeon)
            : base(id, profiles)
        {
            Dungeon = dungeon;
        }
    }
}
