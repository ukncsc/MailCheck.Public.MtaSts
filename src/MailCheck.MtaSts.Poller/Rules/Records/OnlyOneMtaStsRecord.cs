using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Poller.Domain.Errors.Rules;

namespace MailCheck.MtaSts.Poller.Rules.Records
{
    public class OnlyOneMtaStsRecord : IRule<MtaStsRecords>
    {
        public Task<List<AdvisoryMessage>> Evaluate(MtaStsRecords t)
        {
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();
            if (t.Records.Count > 1)
            {
                errors.Add(new OnlyOneMtaStsRecordError());
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 2;
        public bool IsStopRule => false;
    }
}