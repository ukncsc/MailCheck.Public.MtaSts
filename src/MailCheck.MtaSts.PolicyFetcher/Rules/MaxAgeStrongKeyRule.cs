using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Contracts.PolicyFetcher;

namespace MailCheck.MtaSts.PolicyFetcher.Rules
{
    public class MaxAgeStrongKeyRule : IRule<MtaStsPolicyResult>
    {
        private static readonly Guid PolicyMaxAgeIsTooShortEnforce = new Guid("851F6934-F1EC-4825-801A-0DAD55EBEB69");
        private static readonly  Guid PolicyMaxAgeIsTooShortTesting = new Guid("5F39FAF8-68FB-496F-AA3A-C7A866741C7B");
        
        public Task<List<AdvisoryMessage>> Evaluate(MtaStsPolicyResult item)
        {
            List<AdvisoryMessage> messages = new List<AdvisoryMessage>();

            MaxAgeKey maxAgeKey = item.Keys.OfType<MaxAgeKey>().FirstOrDefault();

            if (maxAgeKey != null)
            {
                ModeKey modeKey = item.Keys.OfType<ModeKey>().FirstOrDefault();

                if (modeKey != null && int.TryParse(maxAgeKey.Value, out int maxAge))
                {
                    string modeValue = modeKey.Value?.ToLower() ?? string.Empty;
                    if (modeValue.Equals("enforce") && maxAge < 1209600)
                    {
                        messages.Add(new MtaStsAdvisoryMessage(PolicyMaxAgeIsTooShortEnforce, "mailcheck.mtasts.policyMaxAgeIsTooShortEnforce", MessageType.warning,
                            MtaStsRulesResource.PolicyMaxAgeIsTooShortEnforceMode,
                            MtaStsRulesMarkDownResource.PolicyMaxAgeIsTooShortEnforceMode));
                    }
                    else if (modeValue.Equals("testing") && maxAge < 86400)
                    {
                        messages.Add(new MtaStsAdvisoryMessage(PolicyMaxAgeIsTooShortTesting, "mailcheck.mtasts.policyMaxAgeIsTooShortTesting", MessageType.warning,
                            MtaStsRulesResource.PolicyMaxAgeIsTooShortTestingMode,
                            MtaStsRulesMarkDownResource.PolicyMaxAgeIsTooShortTestingMode));
                    }
                }
            }

            return Task.FromResult(messages);
        }

        public int SequenceNo => 2;
        public bool IsStopRule => false;
    }
}
