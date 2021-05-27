using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Parser
{
    public class InvalidExtensionTagKeyError : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("c01297f7-9582-4cf1-9107-d33768b3726f");

        public InvalidExtensionTagKeyError()
            : base(_Id, AdvisoryType.Error, MtaStsParserErrorMessages.InvalidExtensionTagKeyError, MtaStsParserErrorMarkdown.InvalidExtensionTagKeyError)
        { }
    }
}