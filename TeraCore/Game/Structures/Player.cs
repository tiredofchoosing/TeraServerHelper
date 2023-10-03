namespace TeraCore.Game.Structures
{
    public class Player
    {
        public uint PlayerId { get; init; }
        public string Name { get; init; }
        public byte Level { get; private set; } // TODO increase level
        public PlayerClass Class { get; init; }

        public Player(uint playerId, string name, int level, PlayerClass playerClass)
        {
            PlayerId = playerId;
            Name = name;
            Level = (byte)level;
            Class = playerClass;
        }

        public override string ToString()
        {
            return $"{Name} ({Class} {Level} lvl)";
        }

        //public bool Equals(Player player)
        //{
        //    return PlayerId == player.PlayerId;
        //}
    }
}