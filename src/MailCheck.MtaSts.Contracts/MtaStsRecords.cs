using System.Collections.Generic;

namespace MailCheck.MtaSts.Contracts
{
    public class MtaStsRecords
    {
        public MtaStsRecords(string domain, List<MtaStsRecord> records, int messageSize)
        {
            Domain = domain;
            Records = records;
            MessageSize = messageSize;
        }

        public string Domain { get; }
        public List<MtaStsRecord> Records { get; }
        public int MessageSize { get; }
    }
}