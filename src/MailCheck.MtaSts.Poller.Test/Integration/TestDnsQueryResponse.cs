using System.Collections.Generic;
using DnsClient;
using DnsClient.Protocol;

namespace MailCheck.MtaSts.Poller.Test.Integration
{
    public class TestDnsQueryResponse : IDnsQueryResponse
    {
        public TestDnsQueryResponse(IReadOnlyList<DnsResourceRecord> answers)
        {
            Answers = answers;
        }

        public IReadOnlyList<DnsQuestion> Questions { get; }
        public IReadOnlyList<DnsResourceRecord> Additionals { get; }
        public IEnumerable<DnsResourceRecord> AllRecords { get; }
        public IReadOnlyList<DnsResourceRecord> Answers { get; }
        public IReadOnlyList<DnsResourceRecord> Authorities { get; }
        public string AuditTrail { get; }
        public string ErrorMessage { get; set; }
        public bool HasError { get; set; }
        public DnsResponseHeader Header { get; }
        public int MessageSize { get; }
        public NameServer NameServer { get; }
        public DnsQuerySettings Settings { get; }
    }
}