using System.Text.Json.Serialization;

namespace TeraCore.Game.Structures
{
    public class Player
    {
        [JsonIgnore]
        public ulong EntityId { get; set; }
        public uint PlayerId { get; init; }
        public string Name { get; init; }
        public int Level { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlayerClass Class { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlayerRace Race { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PlayerGender Gender { get; init; }

        public Player(ulong entity, uint playerId, string name, int level, PlayerClass playerClass, PlayerRace race, PlayerGender gender)
        {
            EntityId = entity;
            PlayerId = playerId;
            Name = name;
            Level = level;
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