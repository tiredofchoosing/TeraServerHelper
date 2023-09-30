using System;
using System.Collections.Generic;
using System.Linq;
using TeraCore.Game.Structures;
using TeraCore.Game;

namespace TeraPartyMonitor.Structures
{
    internal class TeraDataPools
    {
        public TeraDataPool<Client> ClientCollection { get; init; }
        public TeraDataPool<Player> PlayerCollection { get; init; }
        public TeraDataPool<Party> PartyCollection { get; init; }
        public TeraDataPool<PartyInfo> PartyInfoCollection { get; init; }
        public TeraDataPool<PartyMatching> PartyMatchingCollection { get; init; }
        protected TeraDataPool<Player> CachedPlayers { get; init; }

        public TeraDataPools(int capacity = 64)
        {
            ClientCollection = new(capacity);
            PlayerCollection = new(capacity);
            PartyCollection = new(capacity);
            PartyInfoCollection = new(capacity);
            PartyMatchingCollection = new(capacity);
            CachedPlayers = new(capacity);

            ClientCollection.ItemRemoved += ClientCollection_ItemRemoved;
            PlayerCollection.ItemRemoved += PlayerCollection_ItemRemoved;
        }

        #region Event Handlers

        private void PlayerCollection_ItemRemoved(Player player)
        {
            if (GetPartyByPlayer(player) == null)
                return;

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

        public PartyMatching? GetPartyMatchingByParty(Party party)
        {
            if (party == null)
                throw new ArgumentNullException(nameof(party));

            try
            {
                return PartyMatchingCollection.SingleOrDefault(p => p.Party.Equals(party));
            }
            catch
            {
                throw new Exception($"Party ({party}) has more than one PartyMatching");
            }
        }

        #endregion
    }
}
