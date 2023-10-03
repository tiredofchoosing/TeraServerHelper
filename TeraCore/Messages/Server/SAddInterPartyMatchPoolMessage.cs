using System.Diagnostics;
using TeraCore.Game.Structures;

namespace TeraCore.Game.Messages
{
    public class SAddInterPartyMatchPoolMessage : ParsedMessage
    {
        internal SAddInterPartyMatchPoolMessage(TeraMessageReader reader) : base(reader)
        {
            var instanceCount = reader.ReadUInt16();
            var instancePointer = reader.ReadUInt16();
            var playerCount = reader.ReadUInt16();
            var playerPointer = reader.ReadUInt16();

            reader.Skip(4); // estimated wait time - 2 or 4 bytes

            MatchingType = (MatchingTypes)reader.ReadByte();

            //reader.Skip(2);

            for (var i = 0; i < instanceCount; i++)
            {
                reader.BaseStream.Position = instancePointer - 4;

                var selfPointer = reader.ReadUInt16();
                Debug.Assert(instancePointer == selfPointer);

                instancePointer = reader.ReadUInt16();

                var instanceId = reader.ReadUInt32();
                MatchingInstance instance = MatchingType switch
                {
                    MatchingTypes.Dungeon => new Dungeon(instanceId),
                    MatchingTypes.Battleground => new Battleground(instanceId),
                    _ => throw new MatchingTypesInvalidEnumArgumentException(MatchingType)
                };
                Instances.Add(instance);
            }

            for (var i = 0; i < playerCount; i++)
            {
                reader.BaseStream.Position = playerPointer - 4;

                var selfPointer = reader.ReadUInt16();
                Debug.Assert(playerPointer == selfPointer);

                playerPointer = reader.ReadUInt16();
                var namePointer = reader.ReadUInt16();
                var isLeader = reader.ReadBoolean();
                var role = (PlayerPartyRoles)reader.ReadUInt16();

                //reader.Skip(2);

                reader.BaseStream.Position = namePointer - 4;
                var name = reader.ReadTeraString();
                Profiles.Add(new MatchingProfile(name, isLeader, role));
            }
        }

        public MatchingTypes MatchingType { get; init; }
        public IList<MatchingInstance> Instances { get; init; } = new List<MatchingInstance>();
        public IList<MatchingProfile> Profiles { get; init; } = new List<MatchingProfile>();
    }
}