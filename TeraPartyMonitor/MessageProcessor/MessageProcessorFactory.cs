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
                LoginServerMessage => new SLoginProcessor(message, client, _dataPools),
                S_RETURN_TO_LOBBY => new SReturnToLobbyProcessor(message, client, _dataPools),
                S_CHANGE_EVENT_MATCHING_STATE => new SChangeEventMatchingStateProcessor(message, client, _dataPools),

                C_REGISTER_PARTY_INFO => new CRegisterPartyInfoProcessor(message, client, _dataPools),
                C_UNREGISTER_PARTY_INFO => new CUnregisterPartyInfoProcessor(message, client, _dataPools),

                _ => throw new ArgumentException($"No mapping for message type: {message.GetType()}")
            }; ;
        }
    }
}
