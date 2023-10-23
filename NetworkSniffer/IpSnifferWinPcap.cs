// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PacketDotNet;
using PacketDotNet.Utils;
using SharpPcap;
using SharpPcap.LibPcap;

namespace NetworkSniffer
{
    public class IpSnifferWinPcap : IpSniffer
    {
        private readonly string _pcapFilter;
        private readonly int? _bufferSize;
        private readonly IEnumerable<LibPcapLiveDevice> _devices;
        private volatile uint _droppedPackets;
        private volatile uint _interfaceDroppedPackets;
        private DateTime _nextCheck;

        public event Action<string>? Warning;

        public IpSnifferWinPcap(string pcapFilter, IEnumerable<string>? deviceFilters = null)
        {
            var devices = LibPcapLiveDeviceList.Instance;

            if (deviceFilters != null && deviceFilters.Any())
                _devices = devices.Where(d => deviceFilters.Any(f => d.Description.Contains(f)));
            else
                _devices = devices;

            if (_devices == null || !_devices.Any())
                throw new Exception("Pcap device list is empty!");

            _pcapFilter = pcapFilter;
            _bufferSize = 1 << 24;
        }

        public IEnumerable<string> Status()
        {
            return _devices.Select(device => $"Device {device.LinkType} {(device.Opened ? "Open" : "Closed")} {device.LastError}\r\n{device}");
        }

        protected override void SetEnabled(bool value)
        {
            if (value)
                Start();
            else
                Finish();
        }

        private void Start()
        {
            foreach (var device in _devices)
            {
                device.OnPacketArrival += PacketArrivalHandler;

                try
                {
                    var config = new DeviceConfiguration
                    {
                        Mode = DeviceModes.Promiscuous,
                        BufferSize = _bufferSize,
                        ReadTimeout = 100,
                        Immediate = true,
                    };
                    device.Open(config);
                }
                catch
                {
                    device.OnPacketArrival -= PacketArrivalHandler;
                    continue;
                }
                device.Filter = _pcapFilter;
                device.StartCapture();
            }

            if (!_devices.Any(d => d.Opened))
                throw new Exception("No pcap device was opened!");
        }

        private void Finish()
        {
            foreach (var device in _devices.Where(d => d.Opened))
            {
                try
                {
                    device.StopCapture();
                }
                //catch
                //{
                //    //ignored
                //    //SharpPcap.PcapException: captureThread was aborted after 00:00:02
                //    //it's normal when there is no traffic while stopping
                //}
                finally
                {
                    device.Close();
                    device.OnPacketArrival -= PacketArrivalHandler;
                }
            }
        }

        protected virtual void OnWarning(string message)
        {
            Warning?.Invoke(message);
        }

        private void PacketArrivalHandler(object sender, PacketCapture e)
        {
            IPv4Packet ipPacket;
            try
            {
                var packet = e.GetPacket();
                if (packet.LinkLayerType != LinkLayers.Null)
                {
                    var linkPacket = Packet.ParsePacket(packet.LinkLayerType, packet.Data);
                    ipPacket = linkPacket.PayloadPacket as IPv4Packet;
                }
                else
                {
                    ipPacket = new IPv4Packet(new ByteArraySegment(packet.Data, 4, packet.Data.Length - 4));
                }
                if (ipPacket == null)
                    return;
            }
            catch
            {
                // ignored bad packet
                return;
            }

            OnPacketReceived(ipPacket);

            var now = DateTime.UtcNow;
            if (now <= _nextCheck)
                return;

            _nextCheck = now + TimeSpan.FromSeconds(20);

            var device = e.Device;
            if (device.Statistics.DroppedPackets == _droppedPackets &&
                device.Statistics.InterfaceDroppedPackets == _interfaceDroppedPackets)
                return;

            _droppedPackets = device.Statistics.DroppedPackets;
            _interfaceDroppedPackets = device.Statistics.InterfaceDroppedPackets;
            OnWarning($"DroppedPackets {device.Statistics.DroppedPackets}, InterfaceDroppedPackets {device.Statistics.InterfaceDroppedPackets}, ReceivedPackets {device.Statistics.ReceivedPackets}");
        }
    }
}