using TeraCore.Game;
using TeraCore.Game.Messages;

namespace TeraPartyMonitor.MessageProcessor
{
    public interface ITeraMessageProcessor
    {
        public event Action MessageProcessed;
        public void Process(ParsedMessage message, Client client);
    }
}
