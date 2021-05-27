using System;
using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.MtaSts.PolicyFetcher.Config
{
    public interface IMtaStsPolicyFetcherConfig
    {
        string SnsTopicArn { get; }
        string NameServer { get; }
        TimeSpan DnsRecordLookupTimeout { get; }
    }

    public class MtaStsPolicyFetcherConfig : IMtaStsPolicyFetcherConfig
    {
        public MtaStsPolicyFetcherConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string SnsTopicArn { get; }
        public TimeSpan DnsRecordLookupTimeout { get; }
        public string NameServer { get; }
    }
}