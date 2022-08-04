using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Contracts.PolicyFetcher;

namespace MailCheck.MtaSts.PolicyFetcher.Rules
{
    public class MxShouldBePresent : IRule<MtaStsPolicyResult>
    {
        public Guid Id = new Guid("ac60b693-3511-4886-a2bd-f9ce3c7d98a7");

        public Task<List<AdvisoryMessage>> Evaluate(MtaStsPolicyResult item)
        {
            List<MxKey> mxKeys = item.Keys.OfType<MxKey>().ToList();

            List<AdvisoryMessage> messages = new List<AdvisoryMessage>();

            if (mxKeys?.Count == 0)
            {
                messages.Add(new MtaStsAdvisoryMessage(Id, "mailcheck.mtasts.noMxPresent", MessageType.error, MtaStsRulesResource.NoMxPresent,
                    MtaStsRulesMarkDownResource.NoMxPresent));
            }

            return Task.FromResult(messages);
        }

        public int SequenceNo => 3;
        public bool IsStopRule => false;
    }
}
