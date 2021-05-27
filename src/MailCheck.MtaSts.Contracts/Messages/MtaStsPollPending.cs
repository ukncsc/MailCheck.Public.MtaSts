using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.MtaSts.Contracts.Messages
{
    public class MtaStsPollPending : Message
    {
        public MtaStsPollPending(string id) : base(id)
        {
        }
    }
}
