using System.Text;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraCore.Sniffing;
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

        static TeraDataPools dataPools = new();

        static void Main(string[] args)
        {
            opCodes = new Dictionary<ushort, string>
            {
                { 45946, "S_LOGIN" },
                { 21252, "S_CHANGE_EVENT_MATCHING_STATE" },
                { 49366, "C_REGISTER_PARTY_INFO" },
                { 37546, "C_UNREGISTER_PARTY_INFO" },
                { 57609, "S_RETURN_TO_LOBBY" },
                //{ 45446, "S_EXIT" },
            };

            opCodeNamer = new(opCodes);
            messageFactory = new(opCodeNamer);
            messageProcessorFactory = new(dataPools);

            var server = new CustomServer("Asura", "RU", "178.250.154.7", 7801);

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
                    if (i > 10)
                    {
                        Console.WriteLine($"{dataPools.PartyInfoCollection.Count} party in lfg");
                        foreach (var partyInfo in dataPools.PartyInfoCollection)
                        {
                            Console.WriteLine($"\t{partyInfo.Party} | \"{partyInfo.Message}\"");
                        }

                        Console.WriteLine($"{dataPools.PartyMatchingCollection.Count} party in matching");
                        foreach (var partyMatching in dataPools.PartyMatchingCollection)
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
                }

                Thread.Sleep(1000);
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