using TeraCore.Game.Structures;
using TeraCore.Game;
using System.ComponentModel;
using System.Numerics;

namespace TeraPartyMonitor.Structures
{
    internal class TeraDataPools
    {
        public event Action<IReadOnlyCollection<PartyMatching>, MatchingTypes> PartyMatchingCollectionChanged;

        protected TeraDataPool<Client> ClientCollection { get; init; }
        protected TeraDataPool<Player> PlayerCollection { get; init; }
        protected TeraDataPool<Party> PartyCollection { get; init; }
        protected TeraDataPool<PartyInfo> PartyInfoCollection { get; init; }
        protected TeraDataPool<PartyMatching> PartyMatchingCollection { get; init; }
        //protected TeraDataPool<Player> CachedPlayers { get; init; }

        private Dictionary<Type, object> lockers = new()
        {
            { typeof(Client), new() },
            { typeof(Player), new() },
            { typeof(Party), new() },
            { typeof(PartyInfo), new() },
            { typeof(PartyMatching), new() }
        };

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

        private void PartyMatchingCollection_ItemChanged(PartyMatching matching1, PartyMatching matching2)
        {
            PartyMatchingCollectionChanged?.Invoke(PartyMatchingCollection.AsReadOnly(), matching1.MatchingType);
        }

        private void PartyMatchingCollection_ItemAdded(PartyMatching matching)
        {
            PartyMatchingCollectionChanged?.Invoke(PartyMatchingCollection.AsReadOnly(), matching.MatchingType);
        }

        private void PartyMatchingCollection_ItemRemoved(PartyMatching matching)
        {
            PartyMatchingCollectionChanged?.Invoke(PartyMatchingCollection.AsReadOnly(), matching.MatchingType);
        }

        private void PartyCollection_ItemRemoved(Party party)
        {
            var partyInfo = GetPartyInfoByParty(party);
            if (partyInfo != null)
            {
                Remove(partyInfo);
            }
        }

        private void PlayerCollection_ItemRemoved(Player player)
        {
            var party = GetPartyByPlayer(player);
            if (party == null)
                return;

            if (party.Players.Count > 1)
                return;

            Remove(party);

            //CachedPlayers.Add(player);
        }

        private void ClientCollection_ItemRemoved(Client client)
        {
            var player = client.CurrentPlayer;
            if (player == null)
                return;

            if (PlayerCollection.Contains(player))
            {
                Remove(player);
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
                    Add(party);
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

        public void Add(Client client)
        {
            Add(ClientCollection, client);
        }

        public void Add(Player player)
        {
            Add(PlayerCollection, player);
        }

        public void Add(Party party)
        {
            Add(PartyCollection, party);
        }

        public void Add(PartyInfo partyInfo)
        {
            Add(PartyInfoCollection, partyInfo);
        }

        public void Add(PartyMatching partyMatching)
        {
            Add(PartyMatchingCollection, partyMatching);
        }

        public void Remove(Client client)
        {
            Remove(ClientCollection, client);
        }

        public void Remove(Player player)
        {
            Remove(PlayerCollection, player);
        }

        public void Remove(Party party)
        {
            Remove(PartyCollection, party);
        }

        public void Remove(PartyInfo partyInfo)
        {
            Remove(PartyInfoCollection, partyInfo);
        }

        public void Remove(PartyMatching partyMatching)
        {
            Remove(PartyMatchingCollection, partyMatching);
        }

        #endregion

        private void Add<T>(TeraDataPool<T> pool, T item)
        {
            lock (lockers[typeof(T)])
            {
                pool.Add(item);
            }
        }
        private void Remove<T>(TeraDataPool<T> pool, T item)
        {
            lock (lockers[typeof(T)])
            {
                pool.Remove(item);
            }
        }
    }
}
