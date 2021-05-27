using System.Collections.Generic;

namespace MailCheck.MtaSts.Poller.Domain
{
    public class MtaStsRecordInfo
    {
        public MtaStsRecordInfo(string domain, List<string> recordParts)
        {
            Domain = domain;
            RecordParts = recordParts;
            Record = string.Join(string.Empty, recordParts);
        }

        public string Domain { get; }
        public List<string> RecordParts { get; }
        public string Record { get; }
    }
}