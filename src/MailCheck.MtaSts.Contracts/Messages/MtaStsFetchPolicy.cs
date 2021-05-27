using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.MtaSts.Contracts.Messages
{
    public class MtaStsFetchPolicy : Message
    {
        public MtaStsFetchPolicy(string id) : base(id)
        {
        }
    }
}
