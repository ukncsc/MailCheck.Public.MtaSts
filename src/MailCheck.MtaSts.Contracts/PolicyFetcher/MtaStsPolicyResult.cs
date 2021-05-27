using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Keys;

namespace MailCheck.MtaSts.Contracts.PolicyFetcher
{
    public class MtaStsPolicyResult
    {
        public MtaStsPolicyResult(
            string rawValue,
            List<Key> keys,
            List<AdvisoryMessage> errors)
        {
            RawValue = rawValue;
            Keys = keys;
            Errors = errors;
        }

        public string RawValue { get; }
        public List<Key> Keys { get; }
        public List<AdvisoryMessage> Errors { get; }
    }
}
