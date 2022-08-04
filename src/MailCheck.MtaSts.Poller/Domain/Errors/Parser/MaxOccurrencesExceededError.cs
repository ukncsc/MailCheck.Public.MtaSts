using System;
using Amazon.Runtime;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Parser
{
    public class MaxOccurrencesExceededError : MtaStsAdvisoryMessage
    {
        private static readonly Guid _id = Guid.Parse("9f97b9d7-b227-4099-b7a1-cd042fed2707");

        public MaxOccurrencesExceededError(string tagKey, int maxOccurrences, int occurrences)
            : base(_id, "mailcheck.mtasts.maxOccurrencesExceeded", MessageType.error, FormatError(tagKey, maxOccurrences, occurrences), null)
        {

        }

        private static string FormatError(string tagKey, int maxOccurrences, int occurrences) => string.Format(MtaStsParserErrorMessages.MaxOccurrencesExceededError, tagKey, maxOccurrences, occurrences);
    }
}
