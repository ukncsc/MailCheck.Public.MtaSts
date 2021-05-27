using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Parser
{
    public class FailedPollError : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("f4806d8c-13aa-44ee-b639-197e8f6d8e51");

        public FailedPollError(string message) : base(_Id, AdvisoryType.Error, message, null)
        {
        }
    }
}

