namespace TeraCore.Game.Structures
{
    public class Party
    {
        public IList<Player> Players { get; private set; }
        public bool IsRaid { get; private set; }
        public Player Leader { get; private set; }
        public int MaxPlayers => IsRaid ? 30 : 5;

        public Party(Player player1, Player player2, bool isRaid = false)
        {
            IsRaid = isRaid;
            Players = new List<Player>(MaxPlayers) { player1, player2 };
            Leader = player1;
        }
        public Party(Player player, bool isRaid = false)
        {
            IsRaid = isRaid;
            Players = new List<Player>(MaxPlayers) { player };
            Leader = player;
        }

        public override string ToString()
        {
            return $"Leader: {Leader}, Players count: {Players.Count}";
        }

        //public bool Equals(Party party)
        //{
        //    return Leader.Equals(party.Leader);
        //}
    }
}
