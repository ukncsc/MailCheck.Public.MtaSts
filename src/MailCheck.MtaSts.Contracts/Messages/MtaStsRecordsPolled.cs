using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.MtaSts.Contracts.Messages
{
    public class MtaStsRecordsPolled : Message
    {
        public MtaStsRecordsPolled(string id, string causationId, MtaStsRecords mtaStsRecords, List<MtaStsAdvisoryMessage> advisoryMessages) : base(id)
        {
            CausationId = causationId;
            MtaStsRecords = mtaStsRecords;
            AdvisoryMessages = advisoryMessages ?? new List<MtaStsAdvisoryMessage>();
        }

        public MtaStsRecords MtaStsRecords { get; }
        public List<MtaStsAdvisoryMessage> AdvisoryMessages { get; }
    }
}