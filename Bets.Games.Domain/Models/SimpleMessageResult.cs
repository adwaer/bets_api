using In.Cqrs.Command;
using In.DataAccess.Entity;

namespace Bets.Games.Domain.Models
{
    public class SimpleMessageResult : HasKeyBase<long>, IMessageResult
    {
        public string Body { get; set; }
        public string Info { get; set; }
        public bool Socceed { get; set; }
        public string Type { get; set; }
    }
}