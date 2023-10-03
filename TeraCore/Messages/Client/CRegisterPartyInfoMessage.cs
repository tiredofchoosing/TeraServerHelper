namespace TeraCore.Game.Messages
{
    public class CRegisterPartyInfoMessage : ParsedMessage
    {
        internal CRegisterPartyInfoMessage(TeraMessageReader reader) : base(reader)
        {
            var offset = reader.ReadUInt16();
            
            IsRaid = reader.ReadBoolean();
            
            reader.BaseStream.Position = offset - 4;
            Message = reader.ReadTeraString();
        }

        public bool IsRaid { get; }
        public string Message { get; }
    }
}