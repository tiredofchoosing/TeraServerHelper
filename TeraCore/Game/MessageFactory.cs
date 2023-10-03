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

        private ParsedMessage Instantiate(string opCodeName, TeraMessageReader reader)
        {
            if (!OpcodeNameToType.TryGetValue(opCodeName, out Type type))
                type = typeof(UnknownMessage);

            var constructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Any, new[] { typeof(TeraMessageReader) }, null);
            if (constructor == null)
                throw new Exception("Constructor not found");
            return (ParsedMessage)constructor.Invoke(new object[] { reader });
        }

        public ParsedMessage Create(Message message)
        {
            var reader = new TeraMessageReader(message, _opCodeNamer);
            var opCodeName = _opCodeNamer.GetName(message.OpCode);
            return Instantiate(opCodeName, reader);
        }

        public MessageFactory(OpCodeNamer opCodeNamer)
        {
            _opCodeNamer = opCodeNamer;

            foreach (var name in OpcodeNameToType.Keys)
            {
                opCodeNamer.GetCode(name);
            }
        }

        private static readonly Dictionary<string, Type> OpcodeNameToType = new Dictionary<string, Type>
            {
                {"S_LOGIN", typeof(SLoginMessage)},
                {"S_RETURN_TO_LOBBY", typeof(SReturnToLobbyMessage)},
                {"S_ADD_INTER_PARTY_MATCH_POOL", typeof(SAddInterPartyMatchPoolMessage)},
                {"S_DEL_INTER_PARTY_MATCH_POOL", typeof(SDelInterPartyMatchPoolMessage)},
                {"S_MODIFY_INTER_PARTY_MATCH_POOL", typeof(SModifyInterPartyMatchPoolMessage)},

                {"C_REGISTER_PARTY_INFO", typeof(CRegisterPartyInfoMessage)},
                {"C_UNREGISTER_PARTY_INFO", typeof(CUnregisterPartyInfoMessage)},
                //{"S_EXIT", typeof(S_EXIT)},
            };
    }
}
