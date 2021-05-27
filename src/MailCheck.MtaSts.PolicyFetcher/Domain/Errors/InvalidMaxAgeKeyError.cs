using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class InvalidMaxAgeKeyError : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("a45edb24-f6e1-440f-b0aa-6ce5d9d07a81");

        public InvalidMaxAgeKeyError() : base(_Id, AdvisoryType.Error, "Invalid max_age key.", ErrorResources.MaxAgeKeyErrorMarkdown)
        {
        }
    }
}