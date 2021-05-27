using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.Poller.Domain
{
    public class MtaStsRecordInfos
    {
        private MtaStsRecordInfos(string domain, List<MtaStsRecordInfo> recordsInfos, int messageSize, AdvisoryMessage error, string nameServer, string auditTrail)
        {
            Domain = domain;
            RecordsInfos = recordsInfos ?? new List<MtaStsRecordInfo>();
            MessageSize = messageSize;
            AdvisoryMessage = error;
            NameServer = nameServer;
            AuditTrail = auditTrail;
        }

        public MtaStsRecordInfos(string domain, AdvisoryMessage error, int messageSize, string nameServer, string auditTrail)
            : this(domain, null, messageSize, error, nameServer, auditTrail)
        {
        }

        public MtaStsRecordInfos(string domain, List<MtaStsRecordInfo> recordsInfos, int messageSize)
            : this(domain, recordsInfos, messageSize, null, null, null)
        {
        }

        public string Domain { get; }
        public List<MtaStsRecordInfo> RecordsInfos { get; }
        public int MessageSize { get; }
        public AdvisoryMessage AdvisoryMessage { get; }
        public string NameServer { get; }
        public bool HasError => AdvisoryMessage != null;
        public string AuditTrail { get; }
    }
}