using System;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.Poller.Domain.Errors.Parser
{
    public class InvalidPolicyVersionIdTagError : AdvisoryMessage
    {
        private static readonly Guid _Id = Guid.Parse("867d3dcf-bd18-44bf-b479-b0919bdd8d87");

        public InvalidPolicyVersionIdTagError()
            : base(_Id, AdvisoryType.Error, MtaStsParserErrorMessages.InvalidPolicyVersionIdValueError, null)
        { }
    }
}