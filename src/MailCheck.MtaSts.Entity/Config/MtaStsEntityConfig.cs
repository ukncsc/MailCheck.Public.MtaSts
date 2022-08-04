using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.MtaSts.Entity.Config
{
    public interface IMtaStsEntityConfig
    {
        string SnsTopicArn { get; }
        int NextScheduledInSeconds { get; }
        string RecordType { get; }
        string WebUrl { get; }
    }

    public class MtaStsEntityConfig : IMtaStsEntityConfig
    {
        public MtaStsEntityConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            NextScheduledInSeconds = environmentVariables.GetAsInt("NextScheduledInSeconds");
            RecordType = "MTASTS";
            WebUrl = environmentVariables.Get("WebUrl");
        }

        public string SnsTopicArn { get; }
        public int NextScheduledInSeconds { get; }
        public string RecordType { get; }
        public string WebUrl { get; }
    }
}
