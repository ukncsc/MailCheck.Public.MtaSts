using System;
using Amazon.Runtime;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Parser
{
    public class InvalidExtensionTagValueError : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("528dc49f-2b9d-4089-a148-3802b3aaceaf");

        public InvalidExtensionTagValueError()
            : base(_Id, AdvisoryType.Error, MtaStsParserErrorMessages.InvalidExtensionTagValueError, MtaStsParserErrorMarkdown.InvalidExtensionTagValueError)
        { }
    }
}