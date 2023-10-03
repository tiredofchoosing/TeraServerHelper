namespace TeraCore.Game.Structures
{
    public class MatchingProfile
    {
        public string Name { get; init; }
        public bool IsLeaderRequired { get; init; }
        public PlayerPartyRoles Role { get; init; }
        public Player? LinkedPlayer { get; set; }

        public MatchingProfile(string name, bool isLeaderRequired, PlayerPartyRoles role)
        {
            Name = name;
            IsLeaderRequired = isLeaderRequired;
            Role = role;
        }

        public override string ToString()
        {
            var pl = IsLeaderRequired ? "[PL]" : "";
            return $"{Name} ({Role}) {pl}";
        }
        public string ToString(bool withoutName)
        {
            if (!withoutName)
                return ToString();

            var pl = IsLeaderRequired ? "[PL]" : "";
            return $"{Role} {pl}";
        }
    }
}