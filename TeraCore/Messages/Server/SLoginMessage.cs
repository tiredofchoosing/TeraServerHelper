using TeraCore.Game.Structures;

namespace TeraCore.Game.Messages
{
    public class SLoginMessage : ParsedMessage
    {
        internal SLoginMessage(TeraMessageReader reader) : base(reader)
        {
            reader.Skip(4);
            int nameOffset = reader.ReadInt16();
            reader.Skip(8);
            int raceGenderClass = reader.ReadInt32();
            EntityId = reader.ReadUInt64();
            //ServerId = reader.ReadUInt32();
            reader.Skip(4);
            PlayerId = reader.ReadUInt32();
            reader.Skip(27);
            Level = reader.ReadInt16();
            reader.BaseStream.Position = nameOffset - 4;
            Name = reader.ReadTeraString();

            raceGenderClass -= 10101;
            Class = (PlayerClass)(raceGenderClass % 100);
            Race = (PlayerRace)(raceGenderClass / 200);
            Gender = (PlayerGender)((raceGenderClass % 200) / 100);
        }

        public ulong EntityId { get; init; }
        //public uint ServerId { get; init; }
        public uint PlayerId { get; init; }
        public int Level { get; init; }
        public string Name { get; init; }
        public PlayerClass Class { get; init; }
        public PlayerRace Race { get; init; }
        public PlayerGender Gender { get; init; }
    }
}