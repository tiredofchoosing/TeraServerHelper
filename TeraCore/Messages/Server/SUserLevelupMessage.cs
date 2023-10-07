﻿namespace TeraCore.Game.Messages
{
    public class SUserLevelupMessage : ParsedMessage
    {
        internal SUserLevelupMessage(TeraMessageReader reader) : base(reader)
        {
            reader.Skip(8); // EntityId
            Level = reader.ReadInt16();
        }

        public int Level { get; init; }
    }
}