using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace TeraCore.Game.Structures
{
    [Serializable]
    public class MatchingProfile
    {
        public string Name { get; init; }
        public bool IsLeaderRequired { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlayerPartyRoles Role { get; init; }

        [JsonIgnore]
        public Player? LinkedPlayer { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlayerClass? Class => LinkedPlayer?.Class;
        public int? Level => LinkedPlayer?.Level;
        public uint? PlayerId => LinkedPlayer?.PlayerId;

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