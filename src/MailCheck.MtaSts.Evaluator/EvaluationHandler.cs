using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Evaluator.Config;
using MailCheck.MtaSts.Evaluator.Explainers;
using MailCheck.MtaSts.Contracts;

namespace MailCheck.MtaSts.Evaluator
{
    public class EvaluationHandler : IHandle<MtaStsRecordsPolled>
    {
        private readonly IMtaStsRecordExplainer _recordExplainer;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IMtaStsEvaluatorConfig _config;

        public EvaluationHandler(
            IMtaStsRecordExplainer recordExplainer,
            IMessageDispatcher dispatcher,
            IMtaStsEvaluatorConfig config)
        {
            _recordExplainer = recordExplainer;
            _dispatcher = dispatcher;
            _config = config;
        }

        public async Task Handle(MtaStsRecordsPolled message)
        {
            foreach (MtaStsRecord mtaStsRecord in message.MtaStsRecords.Records)
            {
                _recordExplainer.Process(mtaStsRecord);
            }

            _dispatcher.Dispatch(new MtaStsRecordsEvaluated(message.Id, message.MtaStsRecords, message.AdvisoryMessages, message.Timestamp), _config.SnsTopicArn);
        }
    }
}