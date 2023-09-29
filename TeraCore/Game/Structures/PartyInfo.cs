namespace TeraCore.Game.Structures
{
    public class PartyInfo
    {
        public Party? Party { get; set; }
        public string Message { get; set; }
        public bool IsRaid { get; set; }

        public PartyInfo(Party party, string info, bool isRaid = false)
        {
            Party = party;
            Message = info;
        }
        public PartyInfo(string info, bool isRaid = false)
        {
            Message = info;
        }
    }
}
