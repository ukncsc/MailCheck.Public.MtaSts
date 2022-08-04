using System;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class InvalidKeyError : MtaStsAdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("045edb24-f6e1-eb24-b00a-6ce5d1027a81");

        public InvalidKeyError(string key) : base(_Id, "mailcheck.mtasts.invalidKey", MessageType.error, $"Unknown {key} key.", ErrorResources.PolicyParseErrorMarkdown)
        {
        }
    }
}