namespace TeraCore.Game.Structures
{
    [Serializable]
    public class Dungeon : MatchingInstance
    {
        public Dungeon(uint id, string name, int lvl) : base(id, name, lvl) { }

        public Dungeon(uint id) : base(id) { }
    }
}
