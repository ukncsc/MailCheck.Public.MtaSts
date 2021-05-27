using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Tags;
using MailCheck.MtaSts.Poller.Domain;
using MailCheck.MtaSts.Poller.Domain.Errors;
using MailCheck.MtaSts.Poller.Domain.Errors.Parser;
using MailCheck.MtaSts.Poller.Rules;

namespace MailCheck.MtaSts.Poller.Parsing
{
    namespace MailCheck.MtaSts.Poller.Parsing
    {
        public interface IMtaStsRecordParser
        {
            EvaluationResult<MtaStsRecord> Parse(MtaStsRecordInfo mtaStsRecordInfo);
        }

        public class MtaStsRecordParser : IMtaStsRecordParser
        {
            private const string TagDelimiter = ";";
            private const string TagPartDelimiter = "=";

            private readonly Dictionary<string, ITagParser> _parsers;
            private readonly IExtensionTagParser _extensionTagParser;

            public MtaStsRecordParser(IEnumerable<ITagParser> parsers, IExtensionTagParser extensionTagParser)
            {
                _extensionTagParser = extensionTagParser;
                _parsers = parsers.ToDictionary(_ => _.TagType, _ => _);
            }

            public EvaluationResult<MtaStsRecord> Parse(MtaStsRecordInfo mtaStsRecordInfo)
            {
                List<Tag> tags = new List<Tag>();
                List<AdvisoryMessage> errors = new List<AdvisoryMessage>();

                string[] tokens = mtaStsRecordInfo.Record
                    .Split(TagDelimiter)
                    .Select(_ => _.Trim())
                    .Where(_ => !string.IsNullOrWhiteSpace(_))
                    .ToArray();

                foreach (string token in tokens)
                {
                    string[] tagParts = token.Split(TagPartDelimiter);

                    if (tagParts.Length == 2)
                    {
                        string tagKey = tagParts[0].ToLower();
                        string tagValue = tagParts[1];

                        if (_parsers.TryGetValue(tagKey, out ITagParser tagParser))
                        {
                            EvaluationResult<Tag> tag = tagParser.Parse(tags, mtaStsRecordInfo.Record, token, tagKey, tagValue);

                            int tagInstanceCount = tags.Count(_ => _.GetType() == tag.Item.GetType());

                            if (tagInstanceCount == tagParser.MaxOccurrences)
                            {
                                tag.AdvisoryMessages.Add(new MaxOccurrencesExceededError(tagKey, tagParser.MaxOccurrences, tagInstanceCount));
                            }

                            tags.Add(tag.Item);
                            errors.AddRange(tag.AdvisoryMessages);
                        }
                        else
                        {
                            EvaluationResult<Tag> tag = _extensionTagParser.Parse(token, tagKey, tagValue); 
                            tags.Add(tag.Item);
                            errors.AddRange(tag.AdvisoryMessages);
                        }
                    }
                    else
                    {
                        tags.Add(new MalformedTag(token));
                        errors.Add(new MalformedTagError(token));
                    }
                }

                MtaStsRecord mtaStsRecord = new MtaStsRecord(mtaStsRecordInfo.Domain, mtaStsRecordInfo.RecordParts, tags);
                return new EvaluationResult<MtaStsRecord>(mtaStsRecord, errors);
            }
        }
    }
}