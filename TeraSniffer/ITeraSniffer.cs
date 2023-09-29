// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using TeraCore.Game;
using TeraCore.Sniffing;

namespace TeraSniffing
{
    public interface ITeraSniffer
    {
        bool Enabled { get; set; }
        event Action<Message> MessageReceived;
        event Action<Server> NewConnection;
    }
}