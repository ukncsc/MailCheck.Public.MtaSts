using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.MtaSts.Contracts.Messages
{
    public class MtaStsRecordsPolled : Message
    {
        public MtaStsRecordsPolled(string id, string causationId, MtaStsRecords mtaStsRecords, List<AdvisoryMessage> advisoryMessages) : base(id)
        {
            CausationId = causationId;
            MtaStsRecords = mtaStsRecords;
            AdvisoryMessages = advisoryMessages ?? new List<AdvisoryMessage>();
        }

        public MtaStsRecords MtaStsRecords { get; }
        public List<AdvisoryMessage> AdvisoryMessages { get; }
    }
}