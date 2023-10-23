using TeraCore.Sniffing;

namespace TeraCore.Game.Messages
{
    // Base class for parsed messages
    public abstract class ParsedMessage : Message
    {
        public string OpCodeName { get; }

        internal ParsedMessage(TeraMessageReader reader)
            : base(reader.Message.Time, reader.Message.Direction, reader.Message.Data)
        {
            OpCodeName = reader.OpCodeName;
        }
    }
}