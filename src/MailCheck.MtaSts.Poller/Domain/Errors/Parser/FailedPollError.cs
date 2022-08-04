using System;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Parser
{
    public class FailedPollError : MtaStsAdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("f4806d8c-13aa-44ee-b639-197e8f6d8e51");

        public FailedPollError(string message) : base(_Id, "mailcheck.mtasts.failedPoll", MessageType.error, message, null)
        {
        }
    }
}

