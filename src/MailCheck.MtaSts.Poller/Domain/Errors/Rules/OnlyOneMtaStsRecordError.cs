using System;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Rules
{
    public class OnlyOneMtaStsRecordError : MtaStsAdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("a33972c3-d6b1-42d2-b75c-0d9030e2aacc");

        public OnlyOneMtaStsRecordError() : base(_Id, "mailcheck.mtasts.onlyOneMtaStsRecord", MessageType.error, MtaStsRuleMarkdown.OnlyOneMtaStsRecordError, null)
        {
        }
    }
}