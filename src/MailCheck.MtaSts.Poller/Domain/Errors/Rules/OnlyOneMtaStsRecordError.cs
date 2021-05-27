using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Rules
{
    public class OnlyOneMtaStsRecordError : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("a33972c3-d6b1-42d2-b75c-0d9030e2aacc");

        public OnlyOneMtaStsRecordError() : base(_Id, AdvisoryType.Error, MtaStsRuleMarkdown.OnlyOneMtaStsRecordError, null)
        {
        }
    }
}