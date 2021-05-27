using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Tags;
using MailCheck.MtaSts.Poller.Domain.Errors.Parser;

namespace MailCheck.MtaSts.Poller.Parsing
{
    public class VersionParser : ITagParser
    {
        private const string ValidValue = "v=STSv1";

        public EvaluationResult<Tag> Parse(List<Tag> tags, string record, string token, string tagKey, string tagValue)
        {
            VersionTag versionTag = new VersionTag(token, tagValue);
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();

            int tagInstanceCount = tags.Count(x => x.GetType() == typeof(VersionTag));

            if (!record.Trim().StartsWith(ValidValue) && tagInstanceCount == 0)
            {
                errors.Add(new InvalidVersionTagError());
            }

            return new EvaluationResult<Tag>(versionTag, errors);
        }

        public string TagType => "v";
        public int MaxOccurrences => 1;
    }
}
