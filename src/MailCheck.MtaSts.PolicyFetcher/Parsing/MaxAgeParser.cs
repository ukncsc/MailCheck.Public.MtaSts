using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.PolicyFetcher.Domain.Errors;

namespace MailCheck.MtaSts.PolicyFetcher.Parsing
{
    public class MaxAgeParser : IKeyParser
    {
        public EvaluationResult<Key> Parse(List<Key> keys, string line, string key, string value)
        {
            MaxAgeKey maxAgeKey = new MaxAgeKey(value, line);
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();

            int keyInstanceCount = keys.Count(x => x.GetType() == typeof(MaxAgeKey));
            bool isNum = Int32.TryParse(value, out int num);

            if (isNum && (num > 0 && num <= 31557600))
            {
                maxAgeKey.Explanation = string.Format(Explanations.MaxAgeKeyValidExplanation, value);
            }
            else
            {
                if (keyInstanceCount == 0) errors.Add(new InvalidMaxAgeKeyError());
                maxAgeKey.Explanation = string.Format(Explanations.MaxAgeKeyInvalidValidExplanation, value);
            }

            return new EvaluationResult<Key>(maxAgeKey, errors);
        }

        public string KeyType => "max_age";
        public int MaxOccurrences => 1;
    }
}
