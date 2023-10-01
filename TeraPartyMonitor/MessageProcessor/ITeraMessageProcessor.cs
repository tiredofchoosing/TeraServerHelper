namespace TeraPartyMonitor.MessageProcessor
{
    public interface ITeraMessageProcessor
    {
        public event Action MessageProcessed;
        public void Process();
    }
}
