using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeraCore.Game.Messages;
using TeraCore.Game;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal abstract class TeraMessageProcessor : ITeraMessageProcessor
    {
        protected ParsedMessage Message { get; init; }
        protected Client Client { get; init; }
        protected TeraDataPools DataPools { get; init; }

        public event Action MessageProcessed;

        public TeraMessageProcessor(ParsedMessage message, Client client, TeraDataPools dataPools)
        {
            Message = message;
            Client = client;
            DataPools = dataPools;
        }

        public abstract void Process();
    }
}
