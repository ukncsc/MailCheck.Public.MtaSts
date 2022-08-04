using System;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class InvalidMxKeyError : MtaStsAdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("2ddb16cc-0bc9-45a8-8262-a204d0b3128d");

        public InvalidMxKeyError() : base(_Id, "mailcheck.mtasts.invalidMxKey", MessageType.error, "Invalid mx key.", ErrorResources.MxKeyErrorMarkdown)
        {
        }
    }
}