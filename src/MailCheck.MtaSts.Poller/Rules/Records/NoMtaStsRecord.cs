using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.MtaSts.Contracts;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Poller.Domain.Errors.Rules;

namespace MailCheck.MtaSts.Poller.Rules.Records
{
    public class NoMtaStsRecord : IRule<MtaStsRecords>
    {
        public Task<List<AdvisoryMessage>> Evaluate(MtaStsRecords t)
        {
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();
            if (!t.Records.Any())
            {
                errors.Add(new NoMtaStsError(t.Domain));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}
