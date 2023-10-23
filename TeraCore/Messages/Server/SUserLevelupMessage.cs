namespace TeraCore.Game.Messages
{
    public class SUserLevelupMessage : ParsedMessage
    {
        internal SUserLevelupMessage(TeraMessageReader reader) : base(reader)
        {
            //reader.Skip(8); // EntityId
            EntityId = reader.ReadUInt64();
            Level = reader.ReadInt16();
        }

        public ulong EntityId { get; init; }
        public int Level { get; init; }
    }
}