namespace Bets.ParserHost.Config
{
    public class ParsingBookmakerSettings
    {
        public bool Enabled { get; set; }
        public string Driver { get; set; }
        public string QueueSubject { get; set; }
        public string Url { get; set; }
    }
}