// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;
using TeraCore.Game.Messages;
using TeraCore.Sniffing;

namespace TeraCore.Game
{
    // Creates a ParsedMessage from a Message
    // Contains a mapping from OpCodeNames to message types and knows how to instantiate those
    // Since it works with OpCodeNames not numeric OpCodes, it needs an OpCodeNamer
    public class MessageFactory
    {
        private readonly OpCodeNamer _opCodeNamer;

        public MessageFactory(OpCodeNamer opCodeNamer)
        {
            _opCodeNamer = opCodeNamer;
        }

        public ParsedMessage? Create(Message message)
        {
            var reader = new TeraMessageReader(message, _opCodeNamer);
            var opCodeName = _opCodeNamer.GetName(message.OpCode);
            var parsedMessage = Instantiate(opCodeName, reader);

            reader.Dispose();

            return parsedMessage;
        }

        private static ParsedMessage? Instantiate(string opCodeName, TeraMessageReader reader)
        {
            return opCodeName switch
            {
                "S_LOGIN" => new SLoginMessage(reader),
                "S_RETURN_TO_LOBBY" => new SReturnToLobbyMessage(reader),
                "S_USER_LEVELUP" => new SUserLevelupMessage(reader),

                "S_ADD_INTER_PARTY_MATCH_POOL" => new SAddInterPartyMatchPoolMessage(reader),
                "S_DEL_INTER_PARTY_MATCH_POOL" => new SDelInterPartyMatchPoolMessage(reader),
                "S_MODIFY_INTER_PARTY_MATCH_POOL" => new SModifyInterPartyMatchPoolMessage(reader),

                "C_REGISTER_PARTY_INFO" => new CRegisterPartyInfoMessage(reader),
                "C_UNREGISTER_PARTY_INFO" => new CUnregisterPartyInfoMessage(reader),

                _ => null
            };
        }
    }
}
