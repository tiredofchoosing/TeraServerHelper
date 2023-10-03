namespace TeraCore.Game.Structures
{
    public abstract class MatchingInstance
    {
        public uint Id { get; init; }
        public string Name { get; init; }
        public int Level { get; init; }

        public MatchingInstance(uint id, string name, int lvl)
        {
            Id = id;
            Name = name;
            Level = lvl;
        }

        public MatchingInstance(uint id)
        {
            Id = id;
            Name = InstanceManager.GetInstatnceName(id);
            Level = InstanceManager.GetInstatnceLevel(id);
        }
    }
}
