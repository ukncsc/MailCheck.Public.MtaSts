using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.PolicyFetcher.Domain.Errors;

namespace MailCheck.MtaSts.PolicyFetcher.Parsing
{
    public class VersionParser : IKeyParser
    {
        private const string ValidValue = "STSv1";

        public EvaluationResult<Key> Parse(List<Key> keys, string line, string key, string value)
        {
            VersionKey versionKey = new VersionKey(value, line);
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();

            int keyInstanceCount = keys.Count(x => x.GetType() == typeof(VersionKey));

            if (!(value == ValidValue))
            {
                if (keyInstanceCount == 0) errors.Add(new InvalidVersionKeyError());
                versionKey.Explanation = string.Format(Explanations.VersionKeyInvalidExplanation, value);
            }
            else
            {
                versionKey.Explanation = Explanations.VersionKeyValidExplanation;
            }

            return new EvaluationResult<Key>(versionKey, errors);
        }

        public string KeyType => "version";
        public int MaxOccurrences => 1;
    }
}
