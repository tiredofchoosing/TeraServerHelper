using System.ComponentModel;

namespace TeraCore.Game.Structures
{
    public enum MatchingTypes
    {
        Dungeon,
        Battleground,
        All // used at least in S_DEL_INTER_PARTY_MATCH_POOL to stop both matchings
    }

    public class MatchingTypesInvalidEnumArgumentException : InvalidEnumArgumentException
    {
        public MatchingTypesInvalidEnumArgumentException(MatchingTypes type)
            : base(nameof(type), (int)type, typeof(MatchingTypes)) { }
    }
}