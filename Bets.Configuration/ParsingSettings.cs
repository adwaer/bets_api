using Bets.Configuration.Enums;

namespace Bets.Configuration
{
    public class ParsingSettings : ParsingQueueSettings
    {
        public ParsingBookmakerSettings OneXBet { get; set; }
        public ParsingBookmakerSettings Winline { get; set; }
    }

    public class ParsingQueueSettings
    {
        public SportType SportType { get; set; }
        public string QueueSubject { get; set; }
    }
}
