// Copyright (c) CodesInChaos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace NetworkSniffer
{
    // Only works when WinPcap is installed
    public class IpSnifferWinPcap : IpSniffer
    {
        private readonly string _filter;
        private LibPcapLiveDeviceList _devices;
        private volatile uint _droppedPackets;
        private volatile uint _interfaceDroppedPackets;

        public IpSnifferWinPcap(string filter)
        {
            _filter = filter;
        }

        public IEnumerable<string> Status()
        {
            return _devices.Select(device => string.Format("Device {0} {1} {2}\r\n{3}", device.LinkType, device.Opened ? "Open" : "Closed", device.LastError, device));
        }

        public int? BufferSize { get; set; }

        protected override void SetEnabled(bool value)
        {
            if (value)
                Start();
            else
                Finish();
        }

        private static bool IsInteresting(LibPcapLiveDevice device)
        {
            return true;
        }

        private void Start()
        {
            Debug.Assert(_devices == null);
            try
            {
                _devices = LibPcapLiveDeviceList.New();
            }
            catch (DllNotFoundException ex)
            {
                throw new NetworkSniffingException("WinPcap is not installed", ex);
            }
            var interestingDevices = _devices.Where(IsInteresting);
            foreach (var device in interestingDevices)
            {
                device.OnPacketArrival += device_OnPacketArrival;
                device.Open(DeviceModes.Promiscuous, 1000);
                device.Filter = _filter;
                //if (BufferSize != null)
                //    device.KernelBufferSize = (uint)BufferSize.Value;
                device.StartCapture();
            }
        }

        private void Finish()
        {
            Debug.Assert(_devices != null);
            foreach (var device in _devices.Where(device => device.Opened))
            {
                device.StopCapture();
                device.Close();
            }
            _devices = null;
        }

        public event Action<string> Warning;

        protected virtual void OnWarning(string obj)
        {
            Warning?.Invoke(obj);
        }

        void device_OnPacketArrival(object sender, PacketCapture e)
        {
            var linkPacket = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);

            var ipPacket = linkPacket.PayloadPacket as IPPacket;
            if (ipPacket == null)
                return;

            var ipData = ipPacket.BytesSegment;
            var ipData2 = new ArraySegment<byte>(ipData.Bytes, ipData.Offset, ipData.Length);

            OnPacketReceived(ipData2);

            var device = (LibPcapLiveDevice)sender;
            if (device.Statistics.DroppedPackets != _droppedPackets || device.Statistics.InterfaceDroppedPackets != _interfaceDroppedPackets)
            {
                _droppedPackets = device.Statistics.DroppedPackets;
                _interfaceDroppedPackets = device.Statistics.InterfaceDroppedPackets;
                OnWarning(string.Format("DroppedPackets {0}, InterfaceDroppedPackets {1}", device.Statistics.DroppedPackets, device.Statistics.InterfaceDroppedPackets));
            }
        }
    }
}
