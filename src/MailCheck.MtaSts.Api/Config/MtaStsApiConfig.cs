using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.MtaSts.Api.Config
{
    public interface IMtaStsApiConfig
    {
        string MicroserviceOutputSnsTopicArn { get; }
        string SnsTopicArn { get; }
    }

    public class MtaStsApiConfig : IMtaStsApiConfig
    {
        public MtaStsApiConfig(IEnvironmentVariables environmentVariables)
        {
            MicroserviceOutputSnsTopicArn = environmentVariables.Get("MicroserviceOutputSnsTopicArn");
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string MicroserviceOutputSnsTopicArn { get; }
        public string SnsTopicArn { get; }
    }
}
