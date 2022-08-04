using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.Contracts.Messages
{
    public class MtaStsAdvisoryMessage : AdvisoryMessage
    {
        public string Name { get; set; }

        public MtaStsAdvisoryMessage(Guid id, string name, MessageType messageType, string text, string markDown) : base(id, messageType, text, markDown)
        {
            Name = name;
        }
    }
}
