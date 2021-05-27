using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class InvalidKeyError : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("l45edb24-f6e1-eb24-b0sa-6ce5d1027a81");

        public InvalidKeyError(string key) : base(_Id, AdvisoryType.Error, $"Unknown {key} key.", ErrorResources.PolicyParseErrorMarkdown)
        {
        }
    }
}