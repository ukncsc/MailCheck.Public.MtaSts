using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.Contracts.PolicyFetcher;

namespace MailCheck.MtaSts.PolicyFetcher.Rules
{
    public class MtaStsShouldBeEnforced : IRule<MtaStsPolicyResult>
    {
        public Guid Id = new Guid("5D35C65E-D853-43EF-B1DA-F3473603200B");

        public Task<List<AdvisoryMessage>> Evaluate(MtaStsPolicyResult item)
        {
            ModeKey modeKey = item.Keys.OfType<ModeKey>().FirstOrDefault();

            List<AdvisoryMessage> messages = new List<AdvisoryMessage>();

            if (modeKey?.Value != null && !modeKey.Value.ToLower().Equals("enforce"))
            {
                messages.Add(new AdvisoryMessage(Id, AdvisoryType.Warning, MtaStsRulesResource.PolicyNotEnforced,
                    MtaStsRulesMarkDownResource.PolicyNotEnforced));
            }

            return Task.FromResult(messages);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => true;
    }
}
