using System;
using System.Collections.Generic;
using System.Text;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.PolicyFetcher.Domain.Errors
{
    public class NoPolicyError : MtaStsAdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("30414943-6579-4E7E-B3DC-CC40AA6AD35D");

        public NoPolicyError() : base(_Id, "mailcheck.mtasts.noMtaStsPolicy", MessageType.warning, $"No MTA-STS policy configured.", string.Empty)
        {
        }
    }
}
