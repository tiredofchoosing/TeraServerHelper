﻿namespace TeraCore.Game.Structures
{
    public class PartyMatching
    {
        public IList<MatchingProfile> MatchingProfiles { get; set; }
        public IList<MatchingInstance> Instances { get; init; }
        public MatchingTypes MatchingType { get; init; }

        public PartyMatching(IList<MatchingProfile> profiles, IList<MatchingInstance> instances, MatchingTypes type)
        {
            MatchingProfiles = profiles;
            Instances = instances;
            MatchingType = type;
        }

        public override string ToString()
        {
            return $"Type: {MatchingType}, Players count: {MatchingProfiles.Count}, Instances count: {Instances.Count}";
        }
    }
}
