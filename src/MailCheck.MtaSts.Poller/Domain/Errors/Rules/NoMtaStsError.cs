using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Rules
{
    public class NoMtaStsError : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("8ec28d23-67b4-4e62-aca4-094eae3ebeae");

        public NoMtaStsError(string domain) : base(_Id, AdvisoryType.Warning, MtaStsRuleErrorMessages.NoMtsStsError, FormatMarkDown(domain))
        {
        }
        
        private static string FormatMarkDown(string domain) => string.Format(MtaStsRuleMarkdown.NoMtsStsError, domain);
    }
}
