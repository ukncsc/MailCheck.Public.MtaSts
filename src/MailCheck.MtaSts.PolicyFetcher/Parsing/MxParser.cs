using System;
using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.PolicyFetcher.Domain.Errors;

namespace MailCheck.MtaSts.PolicyFetcher.Parsing
{
    public class MxParser : IKeyParser
    {
        public EvaluationResult<Key> Parse(List<Key> keys, string line, string key, string value)
        {
            MxKey mxKey = new MxKey(value, line);
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();

            string trimmedValue = value;
            if (value.StartsWith("*.")) trimmedValue = value.Remove(0, 2);
            if (trimmedValue.EndsWith(".") || Uri.CheckHostName(trimmedValue) == UriHostNameType.Unknown)
            {
                errors.Add(new InvalidMxKeyError());
                mxKey.Explanation = string.Format(Explanations.MxKeyInvalidExplanation, value);
            }
            else
            {
                mxKey.Explanation = string.Format(Explanations.MxKeyValidExplanation, value);
            }

            return new EvaluationResult<Key>(mxKey, errors);
        }

        public string KeyType => "mx";
        public int MaxOccurrences => int.MaxValue;
    }
}
