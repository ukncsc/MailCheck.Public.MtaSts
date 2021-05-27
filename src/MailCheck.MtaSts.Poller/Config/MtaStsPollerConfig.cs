using System;
using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.MtaSts.Poller.Config
{
    public interface IMtaStsPollerConfig
    {
        string SnsTopicArn { get; }
        string NameServer { get; }
        TimeSpan DnsRecordLookupTimeout { get; }
    }

    public class MtaStsPollerConfig : IMtaStsPollerConfig
    {
        public MtaStsPollerConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            DnsRecordLookupTimeout = TimeSpan.FromSeconds(environmentVariables.GetAsLong("DnsRecordLookupTimeoutSeconds"));
            NameServer = environmentVariables.Get("NameServer", false);
        }

        public string SnsTopicArn { get; }
        public TimeSpan DnsRecordLookupTimeout { get; }
        public string NameServer { get; }
    }
}