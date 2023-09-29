namespace TeraCore.Game.Structures
{
    public class Dungeon
    {
        public uint DungeonId { get; init; }
        public string Name { get; init; }

        public Dungeon(uint id, string name)
        {
            DungeonId = id;
            Name = name;
        }

        public Dungeon(uint id)
        {
            DungeonId = id;
            Name = "";
        }
    }
}
