using System.Collections.Generic;
using Bets.Games.Domain.Models;
using Bets.Games.Domain.MQMsgs;
using In.Common;
using In.Cqrs.Nats.Abstract;

namespace Bets.Configuration.Services.Implementations
{
    /// <summary>
    /// 
    /// </summary>
    public class NatsBkMessageReplyFactory : INatsBkMessageReplyFactory
    {
        private readonly ITypeFactory _typeFactory;
        private readonly INatsSerializer _serializer;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="typeFactory"></param>
        /// <param name="serializer"></param>
        public NatsBkMessageReplyFactory(ITypeFactory typeFactory, INatsSerializer serializer)
        {
            _typeFactory = typeFactory;
            _serializer = serializer;
        }

        public List<BkGame> Get(BkMqMessage message)
        {
            var msgType = _typeFactory.Get(message.MsgType);
            if (msgType == null)
            {
                return null;
            }
            
            return _serializer.DeserializeMsg<List<BkGame>>(message.Msg, msgType);
        }
    }
}