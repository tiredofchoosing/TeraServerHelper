namespace TeraCore.Game.Structures
{
    public class PartyMatching
    {
        public Party Party { get; init; }
        public IList<Dungeon> Dungeons { get; init; }
        
        public PartyMatching(Party party, IList<Dungeon> dungeons)
        {
            Party = party;
            Dungeons = dungeons;
        }
    }
}
