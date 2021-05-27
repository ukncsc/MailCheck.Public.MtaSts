using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.Contracts.PolicyFetcher;

namespace MailCheck.MtaSts.PolicyFetcher.Rules
{
    public class MaxAgeStrongKeyRule : IRule<MtaStsPolicyResult>
    {
        private Guid _id = new Guid("22028395-7CAA-41CC-950A-EE4136C1D867");

        public Task<List<AdvisoryMessage>> Evaluate(MtaStsPolicyResult item)
        {
            List<AdvisoryMessage> messages = new List<AdvisoryMessage>();

            MaxAgeKey maxAgeKey = item.Keys.OfType<MaxAgeKey>().FirstOrDefault();

            if (maxAgeKey != null)
            {
                ModeKey modeKey = item.Keys.OfType<ModeKey>().FirstOrDefault();

                int.TryParse(maxAgeKey.Value, out int maxAge);

                if (modeKey != null)
                {
                    if (modeKey.Value.ToLower().Equals("enforce") && maxAge < 1209600)
                    {
                        messages.Add(new AdvisoryMessage(_id, AdvisoryType.Warning,
                            MtaStsRulesResource.PolicyMaxAgeIsTooShort,
                            MtaStsRulesMarkDownResource.PolicyMaxAgeIsTooShortEnforceMode));
                    }
                    else if (modeKey.Value.ToLower().Equals("testing") && maxAge < 86400)
                    {
                        messages.Add(new AdvisoryMessage(_id, AdvisoryType.Warning,
                            MtaStsRulesResource.PolicyMaxAgeIsTooShort,
                            MtaStsRulesMarkDownResource.PolicyMaxAgeIsTooShortTestingMode));
                    }
                }
            }

            return Task.FromResult(messages);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}
