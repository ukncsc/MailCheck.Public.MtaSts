using System;
using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.MtaSts.Contracts.Messages
{
    public class MtaStsRecordsEvaluated : Message
    {
        public MtaStsRecordsEvaluated(string id, MtaStsRecords records, List<AdvisoryMessage> advisoryMessages, DateTime lastUpdated) : base(id)
        {
            Records = records;
            AdvisoryMessages = advisoryMessages;
            LastUpdated = lastUpdated;
        }

        public MtaStsRecords Records { get; }

        public List<AdvisoryMessage> AdvisoryMessages { get; }

        public DateTime LastUpdated { get; }
    }
}