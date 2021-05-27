using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class FailedToFetch : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("6a18c75a-142d-4a64-9305-dfe0e32b1223");

        public FailedToFetch(string text, string markdown) : base(_Id, AdvisoryType.Error, text, markdown)
        {
        }
    }
}
