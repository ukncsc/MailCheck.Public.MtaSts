using System.Collections.Generic;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Tags;

namespace MailCheck.MtaSts.Poller.Parsing
{
    public interface ITagParser
    {
        EvaluationResult<Tag> Parse(List<Tag> tags, string record, string token, string tagKey, string tagValue);
        string TagType { get; }
        int MaxOccurrences { get; }
    }
}
