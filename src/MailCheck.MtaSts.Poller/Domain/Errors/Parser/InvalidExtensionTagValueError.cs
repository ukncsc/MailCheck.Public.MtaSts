using System;
using Amazon.Runtime;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Parser
{
    public class InvalidExtensionTagValueError : MtaStsAdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("528dc49f-2b9d-4089-a148-3802b3aaceaf");

        public InvalidExtensionTagValueError()
            : base(_Id, "mailcheck.mtasts.invalidExtensionTagValue", MessageType.error, MtaStsParserErrorMessages.InvalidExtensionTagValueError, MtaStsParserErrorMarkdown.InvalidExtensionTagValueError)
        { }
    }
}