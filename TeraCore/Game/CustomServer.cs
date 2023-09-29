using System.Net;

namespace TeraCore.Game
{
    public class CustomServer : Server
    {
        public int Port { get; init; }
        public IPEndPoint EndPoint { get; init; }

        public CustomServer(string name, string region, string ip, int port) : base(name, region, ip)
        {
            Port = port;
            EndPoint = IPEndPoint.Parse($"{Ip}:{Port}");
        }
    }
}
