using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.MtaSts.Contracts;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Tags;
using MailCheck.MtaSts.Poller.Domain.Errors.Rules;

namespace MailCheck.MtaSts.Poller.Rules.Record
{
    public class VersionTagIsRequired : IRule<MtaStsRecord>
    {
        public Task<List<AdvisoryMessage>> Evaluate(MtaStsRecord t)
        {
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();

            VersionTag versionTag = t.Tags.OfType<VersionTag>().FirstOrDefault();

            if (versionTag == null)
            {
                errors.Add(new VersionTagRequiredError());
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 3;
        public bool IsStopRule => false;
    }
}