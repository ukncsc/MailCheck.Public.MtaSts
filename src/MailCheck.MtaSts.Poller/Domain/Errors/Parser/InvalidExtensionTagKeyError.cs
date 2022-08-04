using System;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Parser
{
    public class InvalidExtensionTagKeyError : MtaStsAdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("c01297f7-9582-4cf1-9107-d33768b3726f");

        public InvalidExtensionTagKeyError()
            : base(_Id, "mailcheck.mtasts.invalidExtensionTagKey", MessageType.error, MtaStsParserErrorMessages.InvalidExtensionTagKeyError, MtaStsParserErrorMarkdown.InvalidExtensionTagKeyError)
        { }
    }
}