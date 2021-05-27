using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Evaluator.Config;

namespace MailCheck.MtaSts.Evaluator
{
    public class EvaluationHandler : IHandle<MtaStsRecordsPolled>
    {
        private readonly IMtaStsEvaluationProcessor _mtaStsEvaluationProcessor;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IMtaStsEvaluatorConfig _config;

        public EvaluationHandler(IMtaStsEvaluationProcessor mtaStsEvaluationProcessor,
            IMessageDispatcher dispatcher,
            IMtaStsEvaluatorConfig config)
        {
            _mtaStsEvaluationProcessor = mtaStsEvaluationProcessor;
            _dispatcher = dispatcher;
            _config = config;
        }

        public async Task Handle(MtaStsRecordsPolled message)
        {
            if (message.AdvisoryMessages.Count == 0) _dispatcher.Dispatch(new MtaStsFetchPolicy(message.Id), _config.SnsTopicArn);
            List<AdvisoryMessage> advisoryMessages = message.AdvisoryMessages ?? new List<AdvisoryMessage>();
            List<AdvisoryMessage> additionalAdvisoryMessages = await _mtaStsEvaluationProcessor.Process(message.MtaStsRecords);
            advisoryMessages.AddRange(additionalAdvisoryMessages);

            _dispatcher.Dispatch(new MtaStsRecordsEvaluated(message.Id, message.MtaStsRecords, advisoryMessages, message.Timestamp), _config.SnsTopicArn);
        }
    }
}