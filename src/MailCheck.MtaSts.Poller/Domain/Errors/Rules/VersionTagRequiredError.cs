using System;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Rules
{
    public class VersionTagRequiredError : MtaStsAdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("4f76decd-6f21-49d0-98f7-daf2c329da55");

        public VersionTagRequiredError() : base(_Id, "mailcheck.mtasts.versionTagRequired", MessageType.error, MtaStsRuleErrorMessages.VersionRequiredError, null)
        {
        }
    }
}