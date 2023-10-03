using System.Diagnostics;
using TeraCore.Game.Structures;

namespace TeraCore.Game.Messages
{
    public class SModifyInterPartyMatchPoolMessage : ParsedMessage
    {
        internal SModifyInterPartyMatchPoolMessage(TeraMessageReader reader) : base(reader)
        {
            var playerCount = reader.ReadUInt16();
            var playerPointer = reader.ReadUInt16();

            //reader.Skip(4); // ??
            //reader.Skip(4); // estimated wait time - 2 or 4 bytes
            //reader.Skip(4); // ??

            for (var i = 0; i < playerCount; i++)
            {
                reader.BaseStream.Position = playerPointer - 4;

                var selfPointer = reader.ReadUInt16();
                Debug.Assert(playerPointer == selfPointer);

                playerPointer = reader.ReadUInt16();
                var namePointer = reader.ReadUInt16();
                var isLeader = reader.ReadBoolean();

                reader.BaseStream.Position = namePointer - 4;
                var name = reader.ReadTeraString();
                Modifiers.Add((name, isLeader));
            }
        }

        public IList<(string, bool)> Modifiers { get; init; } = new List<(string, bool)>();
    }
}