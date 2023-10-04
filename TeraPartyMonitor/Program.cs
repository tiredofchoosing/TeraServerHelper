using Microsoft.Extensions.Configuration;
using NLog;
using System.Net;
using System.Text;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraCore.Sniffing;
using TeraPartyMonitor.DataSender;
using TeraPartyMonitor.DataSender.Models;
using TeraPartyMonitor.MessageProcessor;
using TeraPartyMonitor.Structures;
using TeraSniffing;

namespace TeraPartyMonitor
{
    internal class Program
    {
        static CustomTeraSniffer teraSniffer;
        static Dictionary<ushort, string> opCodes;
        static OpCodeNamer opCodeNamer;
        static MessageFactory messageFactory;
        static MessageProcessorFactory messageProcessorFactory;

        static Logger logger;
        static Requester dungeonRequester;
        static TeraDataPools dataPools;

        static void Main(string[] args)
        {
            var nlogConfigFile = Path.Combine(Environment.CurrentDirectory, "Properties", "nlog.config");
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(nlogConfigFile);
            logger = LogManager.GetLogger("Main");

            try
            {
                var config = GetConfig();
                var serverString = config["Server"];
                var dungeonApiUrl = config["DungeonApiUrl"];
                //var battlegroundApiUrl = config["BattlegroundApiUrl"];

                if (serverString == null || dungeonApiUrl == null)// || battlegroundApiUrl == null)
                {
                    var sb = new StringBuilder("Config file does not contain required keys: ");
                    if (serverString == null)
                        sb.AppendLine("Server");
                    if (dungeonApiUrl == null)
                        sb.AppendLine("DungeonApiUrl");
                    //if (battlegroundApiUrl == null)
                    //    sb.AppendLine("BattlegroundApiUrl");

                    logger.Fatal(sb.ToString());
                    return;
                }

                CustomServer server;
                if (IPEndPoint.TryParse(serverString, out var ep))
                {
                    server = new CustomServer(ep);
                }
                else
                {
                    logger.Fatal("Could not parse server address.");
                    return;
                }

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

                teraSniffer = new(server);
                teraSniffer.MessageClientReceived += TeraMessageReceived;
                teraSniffer.NewClientConnection += TeraNewConnection;
                teraSniffer.EndClientConnection += TeraEndConnection;
                teraSniffer.Enabled = true;

                dataPools = new();
                dataPools.PartyMatchingCollectionChanged += DataPools_MatchingChanged;
                messageProcessorFactory = new(dataPools);

                dungeonRequester = new(dungeonApiUrl);
                //dungeonRequester.RequestSending += DungeonRequester_RequestSending;
                dungeonRequester.ResponseReceived += DungeonRequester_ResponseReceived;

                var thread = new Thread(new ThreadStart(MainLoop));
                thread.Start();
            }
            catch (Exception e)
            {
                logger.Fatal(e.Message);
                return;
            }
        }

        private static void DungeonRequester_ResponseReceived(bool success, string? errorMessage)
        {
            if (!success)
                logger.Debug(errorMessage);
        }

        private static void DungeonRequester_RequestSending(StringContent content)
        {
            logger.Debug(content);
        }

        private async static void DataPools_MatchingChanged(TeraDataPool<PartyMatching> matchings)
        { 
            int i = 1;
            var dungeons = matchings
                .Where(m => m.MatchingType == MatchingTypes.Dungeon)
                .SelectMany(m => m.Instances.Select(instance => (m.MatchingProfiles, instance)))
                .GroupBy(s => s.instance)
                .Select(g => new DungeonMatchingModel(i++, g.Select(p => p.MatchingProfiles), (Dungeon)g.Key));

            await dungeonRequester.CreateAsync(dungeons);
        }

        private static void MainLoop()
        {
            logger.Info("Sniffing started");
            //int i = 1;
            while (true)
            {
                /*
                if (dataPools.PlayerCollection.Count > 0)
                {
                    if (i > 3)
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
                }*/

                Thread.Sleep(1000);
            }
        }

        private static IConfiguration GetConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("Properties\\config.json", false)
                .Build();
        }

        private static void TeraNewConnection(Client client)
        {
            dataPools.ClientCollection.Add(client);
            logger.Info($"New connectoin from {client.EndPoint.Address}:{client.EndPoint.Port}");
        }

        private static void TeraEndConnection(Client client)
        {
            dataPools.ClientCollection.Remove(client);
            logger.Info($"End connectoin from {client.EndPoint.Address}:{client.EndPoint.Port}");
        }

        private static void TeraMessageReceived(Message message, Client client)
        {
            var msg = messageFactory.Create(message);
            if (msg is UnknownMessage)
                return;

            try
            {
                ProcessParsedMessage(msg, client);
            }
            catch (Exception e)
            {
                logger.Error($"Error while processing ParsedMessage\n{e.Message}");
            }
        }

        private static void ProcessParsedMessage(ParsedMessage message, Client client)
        {
            var processor = messageProcessorFactory.Create(message, client);
            processor.Process();
        }
    }
}