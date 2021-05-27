using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class InvalidVersionKeyError : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("91f2d196-f972-48c0-a0dc-ea46f44a302d");

        public InvalidVersionKeyError() : base(_Id, AdvisoryType.Error, "Invalid version key.", ErrorResources.VersionKeyErrorMarkdown)
        {
        }
    }
}