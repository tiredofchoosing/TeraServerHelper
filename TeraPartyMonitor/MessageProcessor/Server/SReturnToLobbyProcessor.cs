﻿using NLog;
using TeraCore.Game;
using TeraCore.Game.Messages;
using TeraPartyMonitor.Structures;

namespace TeraPartyMonitor.MessageProcessor
{
    internal class SReturnToLobbyProcessor : TeraMessageProcessor
    {
        public SReturnToLobbyProcessor(ParsedMessage message, Client client, TeraDataPools dataPools, NLog.ILogger logger)
            : base(message, client, dataPools, logger) { }

        public override void Process()
        {
            if (Message is SReturnToLobbyMessage)
            {
                if (Client.CurrentPlayer != null)
                {
                    Logger.Debug($"{Client}|Player logout: {Client.CurrentPlayer}.");
                    DataPools.Remove(Client.CurrentPlayer);
                    Client.CurrentPlayer = null;
                }
                else
                    Logger.Warn($"{Client}|Logout without CurrentPlayer property set!");

                return;
            }
        }
    }
}