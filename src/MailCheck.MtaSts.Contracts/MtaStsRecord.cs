using System.Collections.Generic;
using MailCheck.MtaSts.Contracts.Tags;

namespace MailCheck.MtaSts.Contracts
{
    public class MtaStsRecord
    {
        public MtaStsRecord(string domain, List<string> recordsParts, List<Tag> tags)
        {
            Domain = domain;
            Record = string.Join(string.Empty, recordsParts);
            RecordsParts = recordsParts;
            Tags = tags;
        }

        public string Domain { get; }
        public string Record { get; }
        public List<string> RecordsParts { get; }
        public List<Tag> Tags { get; }
    }
}