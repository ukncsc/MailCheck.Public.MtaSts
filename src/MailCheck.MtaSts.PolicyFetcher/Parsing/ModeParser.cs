using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.PolicyFetcher.Domain.Errors;

namespace MailCheck.MtaSts.PolicyFetcher.Parsing
{
    public class ModeParser : IKeyParser
    {
        private string[] ValidValues = { "enforce", "testing", "none" };

        public EvaluationResult<Key> Parse(List<Key> keys, string line, string key, string value)
        {
            ModeKey modeKey = new ModeKey(value, line);
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();

            int keyInstanceCount = keys.Count(x => x.GetType() == typeof(ModeKey));

            if (!(ValidValues.Contains(value)))
            {
                if (keyInstanceCount == 0) errors.Add(new InvalidModeKeyError());
                modeKey.Explanation = Explanations.ModeKeyInvalidExplanation;
            }
            else
            {
                switch (value)
                {
                    case "enforce":
                        modeKey.Explanation = Explanations.ModeKeyEnforceExplanation;
                        break;
                    case "testing":
                        modeKey.Explanation = Explanations.ModeKeyTestingExplanation;
                        break;
                    case "none":
                        modeKey.Explanation = Explanations.ModeKeyNoneExplanation;
                        break;
                }
            }

            return new EvaluationResult<Key>(modeKey, errors);
        }

        public string KeyType => "mode";
        public int MaxOccurrences => 1;
    }
}
