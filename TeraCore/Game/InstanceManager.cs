namespace TeraCore.Game
{
    public static class InstanceManager
    {
        private static Dictionary<uint, (string, int)> dungeons = new();
        private static Dictionary<uint, (string, int)> battlegrounds = new();

        static InstanceManager()
        {
            var d = Path.Combine(Environment.CurrentDirectory, "dungeons.txt");
            var b = Path.Combine(Environment.CurrentDirectory, "battlegrounds.txt");
            if (File.Exists(d))
                dungeons = new(ReadFile(d));

            if (File.Exists(b))
                battlegrounds = new(ReadFile(b));
        }

        public static string GetInstatnceName(uint id)
        {
            if (dungeons.ContainsKey(id))
                return dungeons[id].Item1;

            if (battlegrounds.ContainsKey(id))
                return battlegrounds[id].Item1;

            return string.Empty;
        }

        public static int GetInstatnceLevel(uint id)
        {
            if (dungeons.ContainsKey(id))
                return dungeons[id].Item2;

            if (battlegrounds.ContainsKey(id))
                return battlegrounds[id].Item2;

            return 0;
        }

        private static IEnumerable<KeyValuePair<uint, (string, int)>> ReadFile(string filename)
        {
            var names = File.ReadLines(filename).Select(s => s.Split(',').Select(part => part.Trim()).ToArray())
                      .Select(parts => new KeyValuePair<uint, (string, int)>(uint.Parse(parts[0]), (parts[2], int.Parse(parts[1]))));

            return names;
        }

    }
}
