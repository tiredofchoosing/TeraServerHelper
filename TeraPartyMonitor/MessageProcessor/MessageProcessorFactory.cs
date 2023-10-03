using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class MessageProcessorFactory
    {
        TeraDataPools _dataPools;

        public MessageProcessorFactory(TeraDataPools dataPools)
        {
            _dataPools = dataPools;
        }

        public ITeraMessageProcessor Create(ParsedMessage message, Client client)
        {
            return message switch
            {
                SLoginMessage => new SLoginProcessor(message, client, _dataPools),
                SReturnToLobbyMessage => new SReturnToLobbyProcessor(message, client, _dataPools),
                SAddInterPartyMatchPoolMessage => new SAddInterPartyMatchPoolProcessor(message, client, _dataPools),
                SDelInterPartyMatchPoolMessage => new SDelInterPartyMatchPoolProcessor(message, client, _dataPools),
                SModifyInterPartyMatchPoolMessage => new SModifyInterPartyMatchPoolProcessor(message, client, _dataPools),

                CRegisterPartyInfoMessage => new CRegisterPartyInfoProcessor(message, client, _dataPools),
                CUnregisterPartyInfoMessage => new CUnregisterPartyInfoProcessor(message, client, _dataPools),

                _ => throw new ArgumentException($"No mapping for message type: {message.GetType()}")
            }; ;
        }
    }
}
