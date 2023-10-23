// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PacketDotNet;

namespace NetworkSniffer
{
    public abstract class IpSniffer
    {
        private bool _enabled;

        public event Action<IPv4Packet>? PacketReceived;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    SetEnabled(value);
                    _enabled = value;
                }
            }
        }

        protected virtual void OnPacketReceived(IPv4Packet data)
        {
            PacketReceived?.Invoke(data);
        }

        protected abstract void SetEnabled(bool value);
    }
}