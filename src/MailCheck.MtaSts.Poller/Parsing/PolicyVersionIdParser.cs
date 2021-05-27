using System.Collections.Generic;
using System.Text.RegularExpressions;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Tags;
using MailCheck.MtaSts.Poller.Domain.Errors.Parser;

namespace MailCheck.MtaSts.Poller.Parsing
{
    public class PolicyVersionIdParser : ITagParser
    {
        public EvaluationResult<Tag> Parse(List<Tag> tags, string record, string token, string tagKey, string tagValue)
        {
            PolicyVersionIdTag policyVersionIdTag = new PolicyVersionIdTag(token, tagValue);
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();

            if (tagValue.Trim().Length > 32 || tagValue.Trim().Length == 0)
            {
                errors.Add(new InvalidPolicyVersionIdTagError());
            }

            Regex r = new Regex("^[a-zA-Z0-9]*$");
            if (!r.IsMatch(tagValue))
            {
                errors.Add(new InvalidPolicyVersionIdTagError());
            }

            return new EvaluationResult<Tag>(policyVersionIdTag, errors);
        }

        public string TagType => "id";
        public int MaxOccurrences => 1;
    }
}