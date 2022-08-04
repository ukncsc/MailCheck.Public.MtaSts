using System;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class FailedToParse : MtaStsAdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("4777d49a-f4c3-44f2-b061-ef25304ae858");

        public FailedToParse() :
            base(_Id, "mailcheck.mtasts.failedToParse", MessageType.error, ErrorResources.PolicyParseError, ErrorResources.PolicyParseErrorMarkdown)
        {
        }
    }
}
