using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.MtaSts.Entity.Entity.Notifications
{
    public class MtaStsAdvisorySustained : Message
    {
        public MtaStsAdvisorySustained(string id, List<AdvisoryMessage> messages) : base(id)
        {
            Messages = messages;
        }
        public List<AdvisoryMessage> Messages { get; }
    }
}