using System.ComponentModel;
using System.Net;
using System.Text;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraCore.Sniffing;
using TeraPartyMonitor.MessageProcessor;
using TeraPartyMonitor.Structures;
using TeraSniffing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TeraPartyMonitor
{
    internal class Program
    {
        static CustomTeraSniffer teraSniffer;
        static Dictionary<ushort, string> opCodes;
        static OpCodeNamer opCodeNamer;
        static MessageFactory messageFactory;
        static MessageProcessorFactory messageProcessorFactory;
        static readonly IPEndPoint defaultServerEndPoint = IPEndPoint.Parse("178.250.154.7:7801");

        static TeraDataPools dataPools = new();

        static void Main(string[] args)
        {
            CustomServer? server = null;
            if (args.Length == 1)
            {
                if (IPEndPoint.TryParse(args[0], out var ep))
                {
                    server = new CustomServer(ep);
                }
            }
            else if (args.Length == 2)
            {
                if (IPEndPoint.TryParse($"{args[0]}:{args[1]}", out var ep))
                {
                    server = new CustomServer(ep);
                }
            }
            if (args.Length > 0 && server == null)
            {
                Console.WriteLine("Could not parse ip and port. Using default values.");
            }

            server ??= new CustomServer(defaultServerEndPoint, "Asura");

            opCodes = new Dictionary<ushort, string>
            {
                { 58604, "S_LOGIN" },
                { 54807, "S_RETURN_TO_LOBBY" },
                { 48376, "S_ADD_INTER_PARTY_MATCH_POOL" },
                { 42469, "S_DEL_INTER_PARTY_MATCH_POOL" },
                { 21623, "S_MODIFY_INTER_PARTY_MATCH_POOL" },

                { 23845, "C_REGISTER_PARTY_INFO" },
                { 54412, "C_UNREGISTER_PARTY_INFO" },
                //{ 45446, "S_EXIT" },
            };

            opCodeNamer = new(opCodes);
            messageFactory = new(opCodeNamer);
            messageProcessorFactory = new(dataPools);

            teraSniffer = new CustomTeraSniffer(server);
            teraSniffer.MessageClientReceived += TeraMessageReceived;
            teraSniffer.NewClientConnection += TeraNewConnection;
            teraSniffer.EndClientConnection += TeraEndConnection;
            teraSniffer.Enabled = true;

            var thread = new Thread(new ThreadStart(MainLoop));
            thread.Start();
        }

        private static void MainLoop()
        {
            Console.WriteLine("Sniffing started");
            int i = 1;
            while (true)
            {
                if (dataPools.PlayerCollection.Count > 0)
                {
                    if (i > 20)
                    {
                        //Console.WriteLine($"{dataPools.PartyInfoCollection.Count} party in lfg");
                        //foreach (var partyInfo in dataPools.PartyInfoCollection)
                        //{
                        //    Console.WriteLine($"\t{partyInfo.Party} | \"{partyInfo.Message}\"");
                        //}

                        if (dataPools.PartyMatchingCollection.Count == 0)
                            continue;

                        Console.WriteLine($"{dataPools.PartyMatchingCollection.Count} party in matching");
                        foreach (var partyMatching in dataPools.PartyMatchingCollection)
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"\tPlayers count: {partyMatching.MatchingProfiles.Count}");
                            foreach (var p in partyMatching.MatchingProfiles)
                            {
                                sb.AppendLine($"\t\t{p.LinkedPlayer} | {p.ToString(true)}");
                            }

                            sb.AppendLine($"\tInstances count: {partyMatching.Instances.Count}");
                            foreach (var d in partyMatching.Instances)
                            {
                                sb.AppendLine($"\t\t{d.Id}: {d.Name} | {d.Level}");
                            }
                            Console.WriteLine(sb.ToString());
                        }

                        i = 0;
                    }
                    i++;
                }

                Thread.Sleep(100);
            }
        }

        private static void TeraNewConnection(Client client)
        {
            dataPools.ClientCollection.Add(client);
            Console.WriteLine($"New connectoin from {client.EndPoint.Address}:{client.EndPoint.Port}");
        }

        private static void TeraEndConnection(Client client)
        {
            dataPools.ClientCollection.Remove(client);
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
            var processor = messageProcessorFactory.Create(message, client);
            processor.Process();
        }
    }
}