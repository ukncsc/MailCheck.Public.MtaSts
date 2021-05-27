using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Sqs;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MailCheck.MtaSts.Poller
{
    public class LambdaEntryPoint : SqsTriggeredLambdaEntryPoint
    {
        public LambdaEntryPoint() : base(new StartUp.StartUp())
        {
        }
    }
}
