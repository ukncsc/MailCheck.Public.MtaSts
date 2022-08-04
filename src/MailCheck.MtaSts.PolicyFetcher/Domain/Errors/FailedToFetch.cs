using System;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class FailedToFetch : MtaStsAdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("6a18c75a-142d-4a64-9305-dfe0e32b1223");

        public FailedToFetch(string text, string markdown) : base(_Id, "mailcheck.mtasts.failedToFetch", MessageType.error, text, markdown)
        {
        }
    }
}
