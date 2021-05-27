using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class InvalidModeKeyError : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("36207f30-52ee-44ce-b396-e960f4c5ef24");

        public InvalidModeKeyError() : base(_Id, AdvisoryType.Error, "Invalid mode key.", ErrorResources.ModeKeyErrorMarkdown)
        {
        }
    }
}