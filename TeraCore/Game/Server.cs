namespace TeraCore.Game
{
    public class Server
    {
        public Server(string name, string region, string ip)
        {
            Ip = ip;
            Name = name;
            Region = region;
        }

        public string Ip { get; init; }
        public string Name { get; init; }
        public string Region { get; init; }
    }
}