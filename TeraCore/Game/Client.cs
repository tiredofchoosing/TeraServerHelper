using System.Net;
using TeraCore.Game.Structures;

namespace TeraCore.Game
{
    public class Client
    {
        public IPEndPoint EndPoint { get; init; }
        public Player? CurrentPlayer { get; set; }

        public Client(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
        }

        public override string ToString()
        {
            return $"{EndPoint.Address}:{EndPoint.Port}";
        }
    }
}
