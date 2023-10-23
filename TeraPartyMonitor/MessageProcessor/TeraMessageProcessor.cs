using TeraCore.Game.Messages;
using TeraCore.Game;
using TeraPartyMonitor.Structures;
using NLog;

namespace TeraPartyMonitor.MessageProcessor
{
    internal abstract class TeraMessageProcessor : ITeraMessageProcessor
    {
        protected ParsedMessage Message { get; init; }
        protected Client Client { get; init; }
        protected TeraDataPools DataPools { get; init; }
        protected NLog.ILogger Logger { get; init; }

        public event Action? MessageProcessed;

        public TeraMessageProcessor(ParsedMessage message, Client client, TeraDataPools dataPools, NLog.ILogger logger)
        {
            Message = message;
            Client = client;
            DataPools = dataPools;
            Logger = logger;
        }

        public abstract void Process();
    }
}
