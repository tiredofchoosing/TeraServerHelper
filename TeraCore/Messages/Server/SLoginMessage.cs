﻿using TeraCore.Game.Structures;

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
            //Id = reader.ReadEntityId();
            //ServerId = reader.ReadUInt32();
            reader.Skip(12);
            PlayerId = reader.ReadUInt32();
            reader.Skip(27);
            Level = reader.ReadInt16();
            reader.BaseStream.Position = nameOffset - 4;
            Name = reader.ReadTeraString();

            Class = (PlayerClass)(raceGenderClass % 100 - 1);
        }

        //public EntityId Id { get; private set; }
        //public uint ServerId { get; private set; }
        public uint PlayerId { get; private set; }
        public int Level { get; private set; }
        public string Name { get; private set; }
        public PlayerClass Class { get; }
    }
}