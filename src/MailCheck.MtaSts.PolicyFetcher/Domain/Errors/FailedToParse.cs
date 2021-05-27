using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class FailedToParse : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("4777d49a-f4c3-44f2-b061-ef25304ae858");

        public FailedToParse() :
            base(_Id, AdvisoryType.Error, ErrorResources.PolicyParseError, ErrorResources.PolicyParseErrorMarkdown)
        {
        }
    }
}
