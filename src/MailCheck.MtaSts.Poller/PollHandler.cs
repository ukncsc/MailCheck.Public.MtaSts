using System;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Poller.Config;
using MailCheck.MtaSts.Poller.Domain;
using Microsoft.Extensions.Logging;

namespace MailCheck.MtaSts.Poller
{
    public class PollHandler : IHandle<MtaStsPollPending>
    {
        private readonly IMtaStsProcessor _processor;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IMtaStsPollerConfig _config;
        private readonly ILogger<PollHandler> _log;

        public PollHandler(IMtaStsProcessor processor,
            IMessageDispatcher dispatcher,
            IMtaStsPollerConfig config,
            ILogger<PollHandler> log)
        {
            _processor = processor;
            _dispatcher = dispatcher;
            _config = config;
            _log = log;
        }

        public async Task Handle(MtaStsPollPending message)
        {
            try
            {
                MtaStsPollResult mtaStsPollResult = await _processor.Process(message.Id);

                MtaStsRecordsPolled mtaStsRecordsPolled = new MtaStsRecordsPolled(mtaStsPollResult.Id, message.MessageId, mtaStsPollResult.MtaStsRecords, mtaStsPollResult.AdvisoryMessages);

                _dispatcher.Dispatch(mtaStsRecordsPolled, _config.SnsTopicArn);
            }
            catch (Exception e)
            {
                string error = $"Error occurred polling domain {message.Id}";
                _log.LogError(e, error);
                throw;
            }
        }
    }
}