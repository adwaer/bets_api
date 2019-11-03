using In.Cqrs.Command;

namespace Bets.Web.Config
{
    public class SimpleMessageResult : IMessageResult
    {
        public string Body { get; set; }
        public string Info { get; set; }
        public bool Socceed { get; set; }
        public string Type { get; set; }
    }
}