using System.Runtime.Serialization;
using Bets.Games.Domain.Models;
using Newtonsoft.Json;

namespace Bets.Games.Domain.MQMsgs
{
    [DataContract]
    public class BkMqMessage
    {
        [DataMember]
        public string Msg { get; set; }
        [DataMember]
        public string MsgType { get; set; }

        public BkMqMessage(object msg)
        {
            Msg = msg.GetType().ToString();
            MsgType = JsonConvert.SerializeObject(msg);
        }
    }
}