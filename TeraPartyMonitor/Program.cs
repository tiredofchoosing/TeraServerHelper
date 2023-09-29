using System.Text;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraCore.Sniffing;
using TeraSniffing;

namespace TeraPartyMonitor
{
    internal class Program
    {
        static CustomTeraSniffer teraSniffer;
        static Dictionary<ushort, string> opCodes;
        static OpCodeNamer opCodeNamer;
        static MessageFactory messageFactory;

        static IList<Client> connectionsList;
        static IList<Player> playersPool;
        static IList<Party> partyPool;
        static IList<PartyInfo> partyInfoPool;
        static IList<PartyMatching> partyMatchingPool;

        static void Main(string[] args)
        {
            opCodes = new Dictionary<ushort, string>
            {
                { 45946, "S_LOGIN" },
                { 21252, "S_CHANGE_EVENT_MATCHING_STATE" },
                { 49366, "C_REGISTER_PARTY_INFO" },
                { 37546, "C_UNREGISTER_PARTY_INFO" },
                { 57609, "S_RETURN_TO_LOBBY" },
                { 45446, "S_EXIT" },
            };

            opCodeNamer = new OpCodeNamer(opCodes);
            messageFactory = new MessageFactory(opCodeNamer);

            connectionsList = new List<Client>();
            var server = new CustomServer("Asura", "RU", "178.250.154.7", 7801);

            teraSniffer = new CustomTeraSniffer(server);
            teraSniffer.MessageClientReceived += TeraMessageReceived;
            teraSniffer.NewClientConnection += TeraNewConnection;
            teraSniffer.EndClientConnection += TeraEndConnection;
            teraSniffer.Enabled = true;

            playersPool = new List<Player>(128);
            partyPool = new List<Party>(64);
            partyInfoPool = new List<PartyInfo>(64);
            partyPool = new List<Party>(64);
            partyMatchingPool = new List<PartyMatching>(64);

            var thread = new Thread(new ThreadStart(MainLoop));
            thread.Start();

        }

        private static void MainLoop()
        {
            Console.WriteLine("Sniffing started");
            int i = 1;
            while (true)
            {
                if (connectionsList.Count > 0 && i > 10)
                {
                    Console.WriteLine($"{partyInfoPool.Count} party in lfg");
                    foreach (var partyInfo in partyInfoPool)
                    {
                        Console.WriteLine($"\t{partyInfo.Party} | \"{partyInfo.Message}\"");
                    }

                    Console.WriteLine($"{partyMatchingPool.Count} party in matching");
                    foreach (var partyMatching in partyMatchingPool)
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine($"\t{partyMatching.Party}");
                        sb.AppendLine($"\tDungeons count: {partyMatching.Dungeons.Count}\n");

                        foreach (var d in partyMatching.Dungeons)
                        {
                            sb.AppendLine($"\t\t{d.DungeonId}");
                        }
                        Console.WriteLine(sb.ToString());
                    }
                    i = 0;
                }

                i++;
                Thread.Sleep(1000);
            }
        }

        private static void TeraNewConnection(Client client)
        {
            connectionsList.Add(client);
            Console.WriteLine($"New connectoin from {client.EndPoint.Address}:{client.EndPoint.Port}");
        }

        private static void TeraEndConnection(Client client)
        {
            connectionsList.Remove(client);
            Console.WriteLine($"End connectoin from {client.EndPoint.Address}:{client.EndPoint.Port}");
        }

        private static void TeraMessageReceived(Message message, Client client)
        {
            var msg = messageFactory.Create(message);
            if (msg is UnknownMessage)
                return;

            Console.WriteLine($"{message.Time}: {opCodeNamer.GetName(message.OpCode)}");
            try
            {
                ProcessParsedMessage(msg, client);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while processing ParsedMessage\n{e.Message}");
            }
        }

        private static void ProcessParsedMessage(ParsedMessage message, Client client)
        {


            switch (message)
            {
                case LoginServerMessage m:
                    {
                        var player = new Player(m.PlayerId, m.Name, m.Level, m.Class);
                        playersPool.Add(player);
                        client.CurrentPlayer = player;
                        Console.WriteLine($"{client.EndPoint.Address}:{client.EndPoint.Port}: New player has log on: {player}");
                        return;
                    }

                case S_RETURN_TO_LOBBY:
                    {
                        if (client.CurrentPlayer != null)
                        {
                            playersPool.Remove(client.CurrentPlayer);
                            Console.WriteLine($"Player has log off: {client.CurrentPlayer}");
                            client.CurrentPlayer = null;
                        }
                        return;
                    }

                case S_EXIT:
                    {
                        if (client.CurrentPlayer != null)
                        {
                            playersPool.Remove(client.CurrentPlayer);
                            connectionsList.Remove(client);
                            Console.WriteLine("Client exit");
                        }
                        return;
                    }

                case C_REGISTER_PARTY_INFO m:
                    {
                        var player = client.CurrentPlayer;
                        var party = GetOrCreatePartyByPlayer(player);

                        var partyInfo = GetPartyInfoByParty(party);
                        if (partyInfo != null)
                        {
                            partyInfoPool.Remove(partyInfo);
                            partyInfo = null;
                        }
                        partyInfo = new PartyInfo(party, m.Message, m.IsRaid);
                        partyInfoPool.Add(partyInfo);
                        return;
                    }

                case C_UNREGISTER_PARTY_INFO:
                    {
                        var player = client.CurrentPlayer;
                        var party = GetPartyByPlayer(player);
                        var partyInfo = GetPartyInfoByParty(party);
                        partyInfoPool.Remove(partyInfo);
                        return;
                    }

                case S_CHANGE_EVENT_MATCHING_STATE m:
                    {
                        var player = client.CurrentPlayer;
                        var party = GetOrCreatePartyByPlayer(player);
                        var partyMatching = GetPartyMatchingByParty(party);

                        if (partyMatching == null && m.Searching)
                        {
                            var dungeons = new List<Dungeon>(m.MatchingEvents.Count);
                            foreach (var d in m.MatchingEvents)
                            {
                                dungeons.Add(new Dungeon(d));
                            }
                            partyMatching = new PartyMatching(party, dungeons);
                            partyMatchingPool.Add(partyMatching);
                        }
                        else if (partyMatching != null && !m.Searching)
                        {
                            partyMatchingPool.Remove(partyMatching);
                        }
                        else
                        {
                            Console.WriteLine("Invalid Party matching process");
                        }
                        return;
                    }
            }

        }

        private static Party? GetPartyByPlayer(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            try
            {
                return partyPool.SingleOrDefault(p => p.Players.Contains(player));
            }
            catch
            {
                throw new Exception($"Player ({player}) is in more than one party");
            }
        }

        private static Party GetOrCreatePartyByPlayer(Player player)
        {
            if (player == null)
                throw new ArgumentNullException(nameof(player));

            try
            {
                var party = partyPool.SingleOrDefault(p => p.Players.Contains(player));
                if (party == null)
                {
                    party = new Party(player);
                    partyPool.Add(party);
                }
                return party;
            }
            catch
            {
                throw new Exception($"Player ({player}) is in more than one party");
            }
        }

        private static PartyInfo? GetPartyInfoByParty(Party party)
        {
            if (party == null)
                throw new ArgumentNullException(nameof(party));

            try
            {
                return partyInfoPool.SingleOrDefault(p => p.Party.Equals(party));
            }
            catch
            {
                throw new Exception($"Party ({party}) has more than one PartyInfo");
            }
        }

        private static PartyMatching? GetPartyMatchingByParty(Party party)
        {
            if (party == null)
                throw new ArgumentNullException(nameof(party));

            try
            {
                return partyMatchingPool.SingleOrDefault(p => p.Party.Equals(party));
            }
            catch
            {
                throw new Exception($"Party ({party}) has more than one PartyMatching");
            }
        }
    }
}