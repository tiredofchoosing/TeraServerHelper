using NLog;
using System.Net;
using System.Text;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Game.Structures;
using TeraCore.Sniffing;
using TeraPartyMonitor.DataSender.Models;
using TeraPartyMonitor.MessageProcessor;
using TeraPartyMonitor.Structures;
using TeraSniffing;

namespace TeraPartyMonitor
{
    internal class Program
    {
        static ICustomTeraSniffer teraSniffer;
        static Dictionary<ushort, string> opCodes;
        static OpCodeNamer opCodeNamer;
        static MessageFactory messageFactory;
        static MessageProcessorFactory messageProcessorFactory;

        static NLog.ILogger logger;
        //static Requester dgRequester, bgRequester;
        static TeraDataPools dataPools;
        static IConfiguration config;

        static bool mainLoopflag = true;
        static readonly string configDir = "Config";

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            var nlogConfigFile = Path.Combine(Environment.CurrentDirectory, configDir, "nlog.config");
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(nlogConfigFile);
            logger = LogManager.GetLogger("Main");
            logger.Info("======================");
            logger.Info("Init.");

            try
            {
                config = GetConfig();
                var serverString = config["Server"];
                //var dgApiUrl = config["DungeonApiUrl"];
                //var bgApiUrl = config["BattlegroundApiUrl"];
                var webAppUrls = config["WebAppUrls"];
                var dgRoute = config["DungeonsRoute"];
                var bgRoute = config["BattlegroundsRoute"];
                var playersRoute = config["PlayersRoute"];

                if (string.IsNullOrWhiteSpace(serverString) ||
                    //string.IsNullOrWhiteSpace(dgApiUrl) ||
                    //string.IsNullOrWhiteSpace(bgApiUrl) ||
                    string.IsNullOrWhiteSpace(webAppUrls) ||
                    string.IsNullOrWhiteSpace(dgRoute) ||
                    string.IsNullOrWhiteSpace(bgRoute) ||
                    string.IsNullOrWhiteSpace(playersRoute))
                {
                    var sb = new StringBuilder("Config file does not contain required keys: ");
                    if (string.IsNullOrWhiteSpace(serverString))
                        sb.AppendLine("Server");
                    //if (string.IsNullOrWhiteSpace(dgApiUrl))
                    //    sb.AppendLine("DungeonApiUrl");
                    //if (string.IsNullOrWhiteSpace(bgApiUrl))
                    //    sb.AppendLine("BattlegroundApiUrl");
                    if (string.IsNullOrWhiteSpace(webAppUrls))
                        sb.AppendLine("WebAppUrls");
                    if (string.IsNullOrWhiteSpace(dgRoute))
                        sb.AppendLine("DungeonsRoute");
                    if (string.IsNullOrWhiteSpace(bgRoute))
                        sb.AppendLine("BattlegroundsRoute");
                    if (string.IsNullOrWhiteSpace(playersRoute))
                        sb.AppendLine("PlayersRoute");

                    throw new Exception(sb.ToString());
                }

                CustomServer server;
                if (IPEndPoint.TryParse(serverString, out var ep))
                    server = new CustomServer(ep);
                else
                    throw new Exception("Could not parse server address.");

                var builder = WebApplication.CreateBuilder();
                builder.Configuration.SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile(Path.Combine(configDir, "appsettings.json"));

                var app = builder.Build();
                app.UseHttpsRedirection();
                app.MapGet(dgRoute, GetDungeons);
                app.MapGet(bgRoute, GetBattlegrounds);
                app.MapGet(playersRoute, GetPlayers);

                opCodes = new Dictionary<ushort, string>
                {
                    { 58604, "S_LOGIN" },
                    { 54807, "S_RETURN_TO_LOBBY" },
                    { 48376, "S_ADD_INTER_PARTY_MATCH_POOL" },
                    { 42469, "S_DEL_INTER_PARTY_MATCH_POOL" },
                    { 21623, "S_MODIFY_INTER_PARTY_MATCH_POOL" },
                    { 27768, "S_USER_LEVELUP" },

                    //{ 23845, "C_REGISTER_PARTY_INFO" },
                    //{ 54412, "C_UNREGISTER_PARTY_INFO" },
                    //{ 45446, "S_EXIT" },
                };

                opCodeNamer = new(opCodes);
                messageFactory = new(opCodeNamer);

                teraSniffer = new CustomTeraSniffer(server);
                teraSniffer.NewClientConnection += TeraNewConnection;
                teraSniffer.EndClientConnection += TeraEndConnection;
                teraSniffer.MessageClientReceived += TeraMessageReceived;
                teraSniffer.Warning += TeraWarning;
                teraSniffer.Enabled = true;
                logger.Info("Sniffing started.");

                dataPools = new();
                //dataPools.PartyMatchingCollectionChanged += DataPools_MatchingChanged;
                messageProcessorFactory = new(dataPools, logger);

                //dgRequester = new(dgApiUrl);
                ////dgRequester.RequestSending += DungeonRequester_RequestSending;
                //dgRequester.ResponseReceived += DungeonRequester_ResponseReceived;

                //bgRequester = new(bgApiUrl);
                ////bgRequester.RequestSending += DungeonRequester_RequestSending;
                //bgRequester.ResponseReceived += DungeonRequester_ResponseReceived;

                var thread = new Thread(new ThreadStart(MainLoop));
                thread.Start();

                app.Run(webAppUrls);
            }
            catch (Exception e)
            {
                logger.Fatal(e.Message);
                return;
            }
        }

        private static void MainLoop()
        {
            int i = 0;
            while (mainLoopflag)
            {
                if (i > 6)
                {
                    logger.Debug($"MainLoop|Sniffer enabled: {teraSniffer.Enabled}");
                    i = 0;
                }
                i++;
                Thread.Sleep(5000);
            }
        }

        #region Event Handlers

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            teraSniffer.Enabled = false;
            logger.Info("Sniffing stopped.");
            mainLoopflag = false;
        }

        //private static void DungeonRequester_ResponseReceived(bool success, string? errorMessage)
        //{
        //    if (!success)
        //        logger.Error(errorMessage);
        //}

        //private static void DungeonRequester_RequestSending(StringContent content)
        //{
        //    logger.Debug(content);
        //}

        //private async static void DataPools_MatchingChanged(IReadOnlyCollection<PartyMatching> matchings, MatchingTypes type)
        //{
        //    int i = 1;
        //    var grouping = matchings
        //        .Where(m => m.MatchingType == type)
        //        .SelectMany(m => m.Instances.Select(instance => (m.MatchingProfiles, instance)))
        //        .GroupBy(s => s.instance);

        //    switch (type)
        //    {
        //        case MatchingTypes.Dungeon:
        //            var dungeons = grouping.Select(g => new DungeonMatchingModel(i++, g.Select(p => p.MatchingProfiles), (Dungeon)g.Key));
        //            await dgRequester.CreateAsync(dungeons);
        //            break;

        //        case MatchingTypes.Battleground:
        //            var battlegrounds = grouping.Select(g => new BattlegroundMatchingModel(i++, g.Select(p => p.MatchingProfiles), (Battleground)g.Key));
        //            await bgRequester.CreateAsync(battlegrounds);
        //            break;

        //        default:
        //            throw new MatchingTypesInvalidEnumArgumentException(type);
        //    };
        //}

        private static void TeraWarning(string obj)
        {
            logger.Warn("Sniffer warning|" + obj);
        }

        private static void TeraNewConnection(Client client)
        {
            dataPools.Add(client);
            logger.Info($"New connectoin from {client}.");
        }

        private static void TeraEndConnection(Client client)
        {
            dataPools.Remove(client);
            logger.Info($"End connectoin from {client}.");
        }

        private static void TeraMessageReceived(Message message, Client client)
        {
            var msg = messageFactory.Create(message);
            if (msg is null)
                return;

            try
            {
                ProcessParsedMessage(msg, client);
            }
            catch (Exception e)
            {
                logger.Error($"{client}|Error while processing {msg.GetType()}\n{e.Message}");
            }
        }

        #endregion

        private static void ProcessParsedMessage(ParsedMessage message, Client client)
        {
            var processor = messageProcessorFactory.Create(message, client);
            processor.Process();
        }

        private static IConfiguration GetConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile(Path.Combine(configDir, "config.json"), false)
                .Build();
        }

        private static IEnumerable<DungeonMatchingModel> GetDungeons()
        {
            int i = 1;
            return dataPools.GetPartyMatchings()
                .Where(m => m.MatchingType == MatchingTypes.Dungeon)
                .SelectMany(m => m.Instances.Select(instance => (m.MatchingProfiles, instance)))
                .GroupBy(s => s.instance)
                .Select(g => new DungeonMatchingModel(i++, g.Select(p => p.MatchingProfiles), (Dungeon)g.Key));
        }

        private static IEnumerable<BattlegroundMatchingModel> GetBattlegrounds()
        {
            int i = 1;
            return dataPools.GetPartyMatchings()
                .Where(m => m.MatchingType == MatchingTypes.Battleground)
                .SelectMany(m => m.Instances.Select(instance => (m.MatchingProfiles, instance)))
                .GroupBy(s => s.instance)
                .Select(g => new BattlegroundMatchingModel(i++, g.Select(p => p.MatchingProfiles), (Battleground)g.Key));
        }

        private static IEnumerable<Player> GetPlayers()
        {
            return dataPools.GetPlayers();
        }
    }
}