using System.Collections.Generic;
using System.Linq;
using MailCheck.MtaSts.Contracts;
using MailCheck.Common.Contracts.Advisories;
using Newtonsoft.Json;

namespace MailCheck.MtaSts.Poller.Domain
{
    public class MtaStsPollResult
    {
        [JsonConstructor]
        public MtaStsPollResult(string id, MtaStsRecords mtaStsRecords, List<AdvisoryMessage> advisoryMessages)
        {
            Id = id;
            MtaStsRecords = mtaStsRecords;
            AdvisoryMessages = advisoryMessages ?? new List<AdvisoryMessage>();
        }

        public MtaStsPollResult(MtaStsRecords rptRecordInfos, List<AdvisoryMessage> errors)
            : this(rptRecordInfos.Domain, rptRecordInfos, errors)
        {

        }

        public MtaStsPollResult(string id, params AdvisoryMessage[] errors)
            : this(id, null, errors.ToList())
        {
        }

        public string Id { get; }
        public MtaStsRecords MtaStsRecords { get; }
        public List<AdvisoryMessage> AdvisoryMessages { get; }
    }
}
