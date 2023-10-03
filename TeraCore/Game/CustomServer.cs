using System.Net;

namespace TeraCore.Game
{
    //public class CustomServer : Server
    //{
    //    public int Port { get; init; }
    //    public IPEndPoint EndPoint { get; init; }

    //    public CustomServer(string name, string region, string ip, int port) : base(name, region, ip)
    //    {
    //        Port = port;
    //        EndPoint = IPEndPoint.Parse($"{Ip}:{Port}");
    //    }
    //    public CustomServer(string name, string region, IPEndPoint ip) : base(name, region, ip.Address.ToString())
    //    {
    //        EndPoint = ip;
    //        Port = ip.Port;
    //    }
    //}
    public class CustomServer
    {
        public IPEndPoint EndPoint { get; init; }
        public string? Name { get; init; }

        public CustomServer(IPEndPoint ip, string? name = null)
        {
            EndPoint = ip;
            Name = name;
        }
    }
}
