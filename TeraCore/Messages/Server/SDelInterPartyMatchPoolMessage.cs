using TeraCore.Game.Structures;

namespace TeraCore.Game.Messages
{
    public class SDelInterPartyMatchPoolMessage : ParsedMessage
    {
        internal SDelInterPartyMatchPoolMessage(TeraMessageReader reader) : base(reader)
        {
            MatchingType = (MatchingTypes)reader.ReadByte();
            //reader.Skip(2);

            reader.Close();
        }

        public MatchingTypes MatchingType { get; init; }
    }
}