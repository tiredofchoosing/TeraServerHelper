using TeraCore.Game;
using TeraCore.Game.Messages;

namespace TeraPartyMonitor.MessageProcessor
{
    public class LoginMessageProcessor : ITeraMessageProcessor
    {
        public event Action MessageProcessed;

        public void Process(ParsedMessage message, Client client)
        {
            throw new NotImplementedException();
        }
    }
}
