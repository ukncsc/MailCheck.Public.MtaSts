using System;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class KeyValueNotFound : MtaStsAdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("7afa5ce5-b186-4f56-969d-5e86799085b6");

        public KeyValueNotFound(string rawText) : base(_Id, "mailcheck.mtasts.keyValueNotFound", MessageType.warning, "Incorrect key-value format.", String.Format(ErrorResources.KeyValueErrorMarkdown, rawText))
        {
        }
    }
}
