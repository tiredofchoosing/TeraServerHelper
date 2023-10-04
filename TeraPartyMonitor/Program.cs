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
        static Requester dgRequester, bgRequester;
        static TeraDataPools dataPools;
        static IConfiguration config;

        static void Main(string[] args)
        {
            var nlogConfigFile = Path.Combine(Environment.CurrentDirectory, "Properties", "nlog.config");
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(nlogConfigFile);
            logger = LogManager.GetLogger("Main");
            logger.Info("======================");
            logger.Info("Init.");

            try
            {
                config = GetConfig();
                var serverString = config["Server"];
                var dgApiUrl = config["DungeonApiUrl"];
                var bgApiUrl = config["BattlegroundApiUrl"];

                if (string.IsNullOrWhiteSpace(serverString) || 
                    string.IsNullOrWhiteSpace(dgApiUrl) ||
                    string.IsNullOrWhiteSpace(bgApiUrl))
                {
                    var sb = new StringBuilder("Config file does not contain required keys: ");
                    if (string.IsNullOrWhiteSpace(serverString))
                        sb.AppendLine("Server");
                    if (string.IsNullOrWhiteSpace(dgApiUrl))
                        sb.AppendLine("DungeonApiUrl");
                    if (string.IsNullOrWhiteSpace(bgApiUrl))
                        sb.AppendLine("BattlegroundApiUrl");

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

                dgRequester = new(dgApiUrl);
                //dgRequester.RequestSending += DungeonRequester_RequestSending;
                dgRequester.ResponseReceived += DungeonRequester_ResponseReceived;

                bgRequester = new(bgApiUrl);
                //bgRequester.RequestSending += DungeonRequester_RequestSending;
                bgRequester.ResponseReceived += DungeonRequester_ResponseReceived;

                var thread = new Thread(new ThreadStart(MainLoop));
                thread.Start();
            }
            catch (Exception e)
            {
                logger.Fatal(e.Message);
                return;
            }
        }

        private static void MainLoop()
        {
            logger.Info("Sniffing started.");
            while (true)
            {
                Thread.Sleep(10000);
            }
        }

        private static IConfiguration GetConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile(Path.Combine("Properties", "config.json"), false)
                .Build();
        }

        #region Event Handlers

        private static void DungeonRequester_ResponseReceived(bool success, string? errorMessage)
        {
            if (!success)
                logger.Error(errorMessage);
        }

        private static void DungeonRequester_RequestSending(StringContent content)
        {
            logger.Debug(content);
        }

        private async static void DataPools_MatchingChanged(IReadOnlyCollection<PartyMatching> matchings, MatchingTypes type)
        {
            int i = 1;
            var grouping = matchings
                .Where(m => m.MatchingType == type)
                .SelectMany(m => m.Instances.Select(instance => (m.MatchingProfiles, instance)))
                .GroupBy(s => s.instance);

            switch (type)
            {
                case MatchingTypes.Dungeon:
                    var dungeons = grouping.Select(g => new DungeonMatchingModel(i++, g.Select(p => p.MatchingProfiles), (Dungeon)g.Key));
                    await dgRequester.CreateAsync(dungeons);
                    break;

                case MatchingTypes.Battleground:
                    var battlegrounds = grouping.Select(g => new BattlegroundMatchingModel(i++, g.Select(p => p.MatchingProfiles), (Battleground)g.Key));
                    await bgRequester.CreateAsync(battlegrounds);
                    break;

                default:
                    throw new MatchingTypesInvalidEnumArgumentException(type);
            };
        }

        private static void TeraNewConnection(Client client)
        {
            dataPools.Add(client);
            logger.Info($"New connectoin from {client.EndPoint.Address}:{client.EndPoint.Port}.");
        }

        private static void TeraEndConnection(Client client)
        {
            dataPools.Remove(client);
            logger.Info($"End connectoin from {client.EndPoint.Address}:{client.EndPoint.Port}.");
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

        #endregion

        private static void ProcessParsedMessage(ParsedMessage message, Client client)
        {
            var processor = messageProcessorFactory.Create(message, client);
            processor.Process();
        }
    }
}