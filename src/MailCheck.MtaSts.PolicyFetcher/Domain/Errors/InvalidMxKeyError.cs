using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class InvalidMxKeyError : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("2ddb16cc-0bc9-45a8-8262-a204d0b3128d");

        public InvalidMxKeyError() : base(_Id, AdvisoryType.Error, "Invalid mx key.", ErrorResources.MxKeyErrorMarkdown)
        {
        }
    }
}