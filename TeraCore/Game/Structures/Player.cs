namespace TeraCore.Game.Structures
{
    public class Player
    {
        public uint PlayerId { get; private set; }
        public string Name { get; private set; }
        public byte Level { get; private set; }
        public PlayerClass Class { get; private set; }

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