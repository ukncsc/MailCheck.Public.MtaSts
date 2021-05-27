using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.MtaSts.Entity.Config
{
    public interface IMtaStsEntityConfig
    {
        string SnsTopicArn { get; }
        int NextScheduledInSeconds { get; }
        string RecordType { get; }
    }

    public class MtaStsEntityConfig : IMtaStsEntityConfig
    {
        public MtaStsEntityConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            NextScheduledInSeconds = environmentVariables.GetAsInt("NextScheduledInSeconds");
            RecordType = "MTASTS";
        }

        public string SnsTopicArn { get; }
        public int NextScheduledInSeconds { get; }
        public string RecordType { get; }
    }
}
