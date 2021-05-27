using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.MtaSts.Evaluator.Config
{
    public interface IMtaStsEvaluatorConfig
    {
        string SnsTopicArn { get; }
    }

    public class MtaStsEvaluatorConfig : IMtaStsEvaluatorConfig
    {
        public MtaStsEvaluatorConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string SnsTopicArn { get; }
    }
}