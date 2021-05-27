using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Contracts.PolicyFetcher;

namespace MailCheck.MtaSts.Contracts.Messages
{
    public class MtaStsPolicyFetched : Message
    {
        public MtaStsPolicyFetched(string id, MtaStsPolicyResult mtaStsPolicyResult) : base(id)
        {
            MtaStsPolicyResult = mtaStsPolicyResult;
        }

        public MtaStsPolicyResult MtaStsPolicyResult { get; }
    }
}