using System.Text.Json.Serialization;

namespace TeraCore.Game.Structures
{
    public class Player
    {
        public uint PlayerId { get; init; }
        public string Name { get; init; }
        public byte Level { get; private set; } // TODO increase level

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlayerClass Class { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlayerRace Race { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlayerGender Gender { get; init; }

        public Player(uint playerId, string name, int level, PlayerClass playerClass, PlayerRace race, PlayerGender gender)
        {
            PlayerId = playerId;
            Name = name;
            Level = (byte)level;
            Class = playerClass;
            Race = race;
            Gender = gender;
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