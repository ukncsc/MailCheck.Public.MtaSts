using System;
using Amazon.Runtime;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Parser
{
    public class InvalidVersionTagError : MtaStsAdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("5badca8b-1b53-42b3-9306-bf40e6fd7a2a");

        public InvalidVersionTagError()
            : base(_Id, "mailcheck.mtasts.invalidVersionTag", MessageType.error, MtaStsParserErrorMessages.InvalidVersionValueError, null)
        { }
    }
}