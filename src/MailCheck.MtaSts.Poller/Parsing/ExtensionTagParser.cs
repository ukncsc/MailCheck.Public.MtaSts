using System.Collections.Generic;
using System.Text.RegularExpressions;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Tags;
using MailCheck.MtaSts.Poller.Domain.Errors.Parser;

namespace MailCheck.MtaSts.Poller.Parsing
{
    public interface IExtensionTagParser
    {
        EvaluationResult<Tag> Parse(string token, string tagKey, string tagValue);
    }

    public class ExtensionTagParser : IExtensionTagParser
    {
        public EvaluationResult<Tag> Parse(string token, string tagKey, string tagValue)
        {
            ExtensionTag extensionTag = new ExtensionTag(token, tagValue);
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();

            Regex validExtensionNameStart = new Regex("^[a-zA-Z0-9]*$");
            Regex validExtensionName = new Regex("^[.a-zA-Z0-9_-]*$");
            Regex validExtensionValue = new Regex("^[\x21-\x3A\x3C\x3E-\x7E]*$");


            if (tagKey.Trim().Length > 32 || !validExtensionNameStart.IsMatch(tagKey[0].ToString()) || !validExtensionName.IsMatch(tagKey)) 
            {
                errors.Add(new InvalidExtensionTagKeyError());
            }

            if (!validExtensionValue.IsMatch(tagValue))
            {
                errors.Add(new InvalidExtensionTagValueError());
            }

            return new EvaluationResult<Tag>(extensionTag, errors);
        }
    }
}