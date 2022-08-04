using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.Contracts.PolicyFetcher
{
    public class MtaStsPolicyResult
    {
        public MtaStsPolicyResult(
            string rawValue,
            List<Key> keys,
            List<MtaStsAdvisoryMessage> errors)
        {
            RawValue = rawValue;
            Keys = keys;
            Errors = errors;
        }

        public string RawValue { get; }
        public List<Key> Keys { get; }
        public List<MtaStsAdvisoryMessage> Errors { get; }
    }
}
