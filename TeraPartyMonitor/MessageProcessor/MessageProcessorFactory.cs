using NLog;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class MessageProcessorFactory
    {
        TeraDataPools _dataPools;
        NLog.ILogger _logger;

        public MessageProcessorFactory(TeraDataPools dataPools, NLog.ILogger logger)
        {
            _dataPools = dataPools;
            _logger = logger;
        }

        public ITeraMessageProcessor Create(ParsedMessage message, Client client)
        {
            return message switch
            {
                SLoginMessage => new SLoginProcessor(message, client, _dataPools, _logger),
                SReturnToLobbyMessage => new SReturnToLobbyProcessor(message, client, _dataPools, _logger),
                SUserLevelupMessage => new SUserLevelupProcessor(message, client, _dataPools, _logger),

                SAddInterPartyMatchPoolMessage => new SAddInterPartyMatchPoolProcessor(message, client, _dataPools, _logger),
                SDelInterPartyMatchPoolMessage => new SDelInterPartyMatchPoolProcessor(message, client, _dataPools, _logger),
                SModifyInterPartyMatchPoolMessage => new SModifyInterPartyMatchPoolProcessor(message, client, _dataPools, _logger),

                CRegisterPartyInfoMessage => new CRegisterPartyInfoProcessor(message, client, _dataPools, _logger),
                CUnregisterPartyInfoMessage => new CUnregisterPartyInfoProcessor(message, client, _dataPools, _logger),

                _ => throw new ArgumentException($"No mapping for message type: {message.GetType()}")
            }; ;
        }
    }
}
