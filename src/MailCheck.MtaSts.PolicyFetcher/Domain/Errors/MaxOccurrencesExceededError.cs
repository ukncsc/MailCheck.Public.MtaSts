using System;
using Amazon.Runtime;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class MaxOccurrencesExceededError : MtaStsAdvisoryMessage
    {
        private static readonly Guid _id = Guid.Parse("a847c898-c45d-4180-9f05-0c604ac40644");

        public MaxOccurrencesExceededError(string key, int maxOccurrences, int occurrences)
            : base(_id, "mailcheck.mtasts.policyMaxOccurrencesExceeded", MessageType.error, FormatError(key, maxOccurrences, occurrences), null)
        {

        }

        private static string FormatError(string key, int maxOccurrences, int occurrences) => string.Format(ErrorResources.MaxOccurrencesExceededError, key, maxOccurrences, occurrences);
    }
}
