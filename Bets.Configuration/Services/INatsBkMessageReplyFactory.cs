using System.Collections.Generic;
using Bets.Games.Domain.Models;
using Bets.Games.Domain.MQMsgs;

namespace Bets.Configuration.Services
{
    public interface INatsBkMessageReplyFactory
    {
        List<BkGame> Get(BkMqMessage message);
    }
}