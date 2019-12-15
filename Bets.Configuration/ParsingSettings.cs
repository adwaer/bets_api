namespace Bets.Configuration
{
    public class ParsingSettings : ParsingQueueSettings
    {
        public ParsingBookmakerSettings OneXBet { get; set; }
    }

    public class ParsingQueueSettings
    {
        public string QueueSubject { get; set; }
    }
}