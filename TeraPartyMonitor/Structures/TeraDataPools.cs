using TeraCore.Game.Structures;
using TeraCore.Game;
using System.ComponentModel;

namespace TeraPartyMonitor.Structures
{
    internal class TeraDataPools
    {
        public event Action<TeraDataPool<PartyMatching>> PartyMatchingCollectionChanged;

        public TeraDataPool<Client> ClientCollection { get; init; }
        public TeraDataPool<Player> PlayerCollection { get; init; }
        public TeraDataPool<Party> PartyCollection { get; init; }
        public TeraDataPool<PartyInfo> PartyInfoCollection { get; init; }
        public TeraDataPool<PartyMatching> PartyMatchingCollection { get; init; }
        //protected TeraDataPool<Player> CachedPlayers { get; init; }

        public TeraDataPools(int capacity = 64)
        {
            ClientCollection = new(capacity);
            PlayerCollection = new(capacity);
            PartyCollection = new(capacity);
            PartyInfoCollection = new(capacity);
            PartyMatchingCollection = new(capacity);
            //CachedPlayers = new(capacity);

            ClientCollection.ItemRemoved += ClientCollection_ItemRemoved;
            PlayerCollection.ItemRemoved += PlayerCollection_ItemRemoved;
            PartyCollection.ItemRemoved += PartyCollection_ItemRemoved;
            PartyMatchingCollection.ItemRemoved += PartyMatchingCollection_ItemRemoved;
            PartyMatchingCollection.ItemAdded += PartyMatchingCollection_ItemAdded;
            PartyMatchingCollection.ItemChanged += PartyMatchingCollection_ItemChanged;
        }

        #region Event Handlers

        private void PartyMatchingCollection_ItemChanged(PartyMatching arg1, PartyMatching arg2)
        {
            PartyMatchingCollectionChanged?.Invoke(PartyMatchingCollection);
        }

        private void PartyMatchingCollection_ItemAdded(PartyMatching obj)
        {
            PartyMatchingCollectionChanged?.Invoke(PartyMatchingCollection);
        }

        private void PartyMatchingCollection_ItemRemoved(PartyMatching partyMatching)
        {
            PartyMatchingCollectionChanged?.Invoke(PartyMatchingCollection);
        }

        private void PartyCollection_ItemRemoved(Party party)
        {
            var partyInfo = GetPartyInfoByParty(party);
            if (partyInfo != null)
            {
                PartyInfoCollection.Remove(partyInfo);
            }
        }

        private void PlayerCollection_ItemRemoved(Player player)
        {
            var party = GetPartyByPlayer(player);
            if (party == null)
                return;

            if (party.Players.Count > 1)
                return;

            PartyCollection.Remove(party);

            //CachedPlayers.Add(player);
        }

        private void ClientCollection_ItemRemoved(Client client)
        {
            var player = client.CurrentPlayer;
            if (player == null)
                return;

            if (PlayerCollection.Contains(player))
            {
                PlayerCollection.Remove(player);
            }
        }

        #endregion

        #region Public Methods

        public Party? GetPartyByPlayer(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            try
            {
                return PartyCollection.SingleOrDefault(p => p.Players.Contains(player));
            }
            catch
            {
                throw new Exception($"Player ({player}) is in more than one party");
            }
        }

        public Party GetOrCreatePartyByPlayer(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            try
            {
                var party = PartyCollection.SingleOrDefault(p => p.Players.Contains(player));
                if (party == null)
                {
                    party = new Party(player);
                    PartyCollection.Add(party);
                }
                return party;
            }
            catch
            {
                throw new Exception($"Player ({player}) is in more than one party");
            }
        }

        public PartyInfo? GetPartyInfoByParty(Party party)
        {
            if (party == null)
                throw new ArgumentNullException(nameof(party));

            try
            {
                return PartyInfoCollection.SingleOrDefault(p => p.Party.Equals(party));
            }
            catch
            {
                throw new Exception($"Party ({party}) has more than one PartyInfo");
            }
        }

        public PartyMatching? GetPartyMatchingByPlayer(Player player, MatchingTypes type)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            try
            {
                return PartyMatchingCollection.SingleOrDefault(pm => pm.MatchingType == type &&
                    pm.MatchingProfiles.Any(prof => prof.LinkedPlayer.Equals(player)));
            }
            catch
            {
                throw new Exception($"Player ({player}) is in more than one PartyMatching ({type})");
            }
        }

        public Player? GetPlayerByName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            try
            {
                return PlayerCollection.SingleOrDefault(p => p.Name.Equals(name));
            }
            catch
            {
                throw new Exception($"There are more than one player with the same name: {name}");
            }
        }

        #endregion
    }
}
