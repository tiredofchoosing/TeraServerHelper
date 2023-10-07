using System.Linq;
using TeraCore.Game.Structures;

namespace TeraCore.Game
{
    public static class InstanceManager
    {
        private static Dictionary<uint, (string, int)> dungeonsNameLevel = new();
        private static Dictionary<uint, (string, int)> battlegroundsNameLevel = new();

        private static Dictionary<uint, MatchingInstance> instances = new();

        static InstanceManager()
        {
            var d = Path.Combine(Environment.CurrentDirectory, "Resources", "dungeons.txt");
            var b = Path.Combine(Environment.CurrentDirectory, "Resources", "battlegrounds.txt");

            if (File.Exists(d))
                dungeonsNameLevel = new(ReadFile(d, MatchingTypes.Dungeon));

            if (File.Exists(b))
                battlegroundsNameLevel = new(ReadFile(b, MatchingTypes.Battleground));
        }

        public static Dungeon GetDungeon(uint id)
        {
            if (instances.ContainsKey(id))
                return (Dungeon)instances[id];
            
            var dg = new Dungeon(id);
            instances.Add(id, dg);
            return dg;
        }

        public static Battleground GetBattleground(uint id)
        {
            if (instances.ContainsKey(id))
                return (Battleground)instances[id];

            var bg = new Battleground(id);
            instances.Add(id, bg);
            return bg;
        }

        public static string GetInstatnceName(uint id)
        {
            if (dungeonsNameLevel.ContainsKey(id))
                return dungeonsNameLevel[id].Item1;

            if (battlegroundsNameLevel.ContainsKey(id))
                return battlegroundsNameLevel[id].Item1;

            return string.Empty;
        }

        public static int GetInstatnceLevel(uint id)
        {
            if (dungeonsNameLevel.ContainsKey(id))
                return dungeonsNameLevel[id].Item2;

            if (battlegroundsNameLevel.ContainsKey(id))
                return battlegroundsNameLevel[id].Item2;

            return 0;
        }

        private static IEnumerable<KeyValuePair<uint, (string, int)>> ReadFile(string filename, MatchingTypes type)
        {
            return File.ReadLines(filename).Select(s => s.Split(',').Select(part => part.Trim()).ToArray())
                      .Select(parts => new KeyValuePair<uint, (string, int)>(uint.Parse(parts[0]), (parts[2], int.Parse(parts[1]))));
        }

    }
}
