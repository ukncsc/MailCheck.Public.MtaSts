using System;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Parser
{
    public class MalformedTagError : MtaStsAdvisoryMessage
    {
        private static readonly Guid _id = Guid.Parse("9854d2df-ad58-421f-91a1-e70978850b10");

        public MalformedTagError(string tagValue) : base(_id, "mailcheck.mtasts.malformedTag", MessageType.error, FormatError(tagValue), null)
        {
        }

        private static string FormatError(string itemValue) => string.Format(MtaStsParserErrorMessages.MalformedItemError, itemValue);
    }
}