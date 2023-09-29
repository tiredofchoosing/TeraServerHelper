// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using TeraCore.Game;
using TeraCore.Sniffing;

namespace TeraSniffing
{
    public interface ICustomTeraSniffer
    {
        bool Enabled { get; set; }
        event Action<Client> NewClientConnection;
        event Action<Client> EndClientConnection;
        event Action<Message, Client> MessageClientReceived;
    }
}