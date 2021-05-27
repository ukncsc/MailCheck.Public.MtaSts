using System;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Common.Exception;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Contracts.Entity;
using MailCheck.MtaSts.Contracts.External;
using MailCheck.MtaSts.Entity.Config;
using MailCheck.MtaSts.Entity.Dao;
using MailCheck.Common.Util;
using Microsoft.Extensions.Logging;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.MtaSts.Contracts.Scheduler;
using MailCheck.MtaSts.Entity.Entity.EmailSecurity;
using MailCheck.MtaSts.Entity.Entity.Notifiers;

namespace MailCheck.MtaSts.Entity.Entity
{
    public class MtaStsEntityNewScheduler :
        IHandle<DomainCreated>,
        IHandle<MtaStsRecordExpired>,
        IHandle<MtaStsScheduledReminder>,
        IHandle<MtaStsPolicyFetched>,
        IHandle<DomainDeleted>,
        IHandle<MtaStsRecordsEvaluated>
    {
        private readonly IMtaStsEntityDao _dao;
        private readonly IMtaStsEntityConfig _mtaStsEntityConfig;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IClock _clock;
        private readonly ILogger<MtaStsEntityNewScheduler> _log;
        private readonly IEntityChangedPublisher _entityChangedPublisher;
        private readonly IChangeNotifier _advisoryChangesNotifier;
        private const string ServiceName = "MtaSts";

        public MtaStsEntityNewScheduler(IMtaStsEntityDao dao,
            IMtaStsEntityConfig config,
            IMessageDispatcher dispatcher,
            IClock clock,
            ILogger<MtaStsEntityNewScheduler> log,
            IEntityChangedPublisher entityChangedPublisher,
            IChangeNotifier advisoryChangesNotifier)
        {
            _dao = dao;
            _mtaStsEntityConfig = config;
            _dispatcher = dispatcher;
            _clock = clock;
            _log = log;
            _entityChangedPublisher = entityChangedPublisher;
            _advisoryChangesNotifier = advisoryChangesNotifier;
        }

        public async Task Handle(DomainCreated message)
        {
            string domainName = message.Id.ToLower();
            MtaStsEntityState state = await _dao.Get(domainName);
            if (state != null)
            {
                _log.LogError($"Ignoring {nameof(DomainCreated)} as MtaSts Entity already exists for {domainName}");
                throw new MailCheckException(
                    $"Cannot handle event {nameof(DomainCreated)} as MtaSts Entity already exists for {domainName}.");
            }

            state = new MtaStsEntityState(domainName, 1, MtaStsState.Created, DateTime.UtcNow);
            await _dao.Upsert(state);
            _log.LogInformation($"Created MtaStsEntity for {domainName}.");

            MtaStsPollPending mtaStsPollPending = new MtaStsPollPending(domainName);
            _dispatcher.Dispatch(mtaStsPollPending, _mtaStsEntityConfig.SnsTopicArn);
            _log.LogInformation(
                $"An MtaStsPollPending message for Domain: {domainName} has been dispatched to SnsTopic: {_mtaStsEntityConfig.SnsTopicArn}");
            
            CreateScheduledReminder createScheduledReminder = new CreateScheduledReminder(
                Guid.NewGuid().ToString(),
                ServiceName,
                domainName,
                default);
            
            _dispatcher.Dispatch(createScheduledReminder, _mtaStsEntityConfig.SnsTopicArn);
            _log.LogInformation(
                $"A CreateScheduledReminder message for Domain: {domainName} has been dispatched to SnsTopic: {_mtaStsEntityConfig.SnsTopicArn}");
        }

        public async Task Handle(MtaStsRecordExpired message)
        {
            string domainName = message.Id.ToLower();

            MtaStsEntityState state = await LoadState(domainName, nameof(message));
            state.Version++;
            state.MtaStsState = MtaStsState.PollPending;

            await _dao.Upsert(state);

            MtaStsPollPending mtaStsPollPending = new MtaStsPollPending(domainName);

            _dispatcher.Dispatch(mtaStsPollPending, _mtaStsEntityConfig.SnsTopicArn);
            _log.LogInformation(
                $"An MtaStsPollPending message for Domain: {domainName} has been dispatched to SnsTopic: {_mtaStsEntityConfig.SnsTopicArn}");
        }

        public async Task Handle(MtaStsScheduledReminder message)
        {
            string domainName = message.ResourceId.ToLower();

            MtaStsEntityState state = await LoadState(domainName, nameof(message));
            _log.LogInformation($"Updating MtaStsEntity.MtaStsState from {state.MtaStsState} to {MtaStsState.PollPending} for {domainName}.");

            state.Version++;
            state.MtaStsState = MtaStsState.PollPending;

            await _dao.Upsert(state);
            MtaStsPollPending mtaStsPollPending = new MtaStsPollPending(domainName);
            _dispatcher.Dispatch(mtaStsPollPending, _mtaStsEntityConfig.SnsTopicArn);
            _log.LogInformation(
                $"An MtaStsPollPending message for Domain: {domainName} has been dispatched to SnsTopic: {_mtaStsEntityConfig.SnsTopicArn}");

        }

        public async Task Handle(MtaStsPolicyFetched message)
        {
            string domainName = message.Id.ToLower();

            MtaStsEntityState state = await LoadState(domainName, nameof(message));
            _advisoryChangesNotifier.Handle(domainName, state.Policy?.Errors, message.MtaStsPolicyResult.Errors);

            state.Policy = message.MtaStsPolicyResult;
            state.Version++;

            await _dao.Upsert(state);

            _entityChangedPublisher.Publish(domainName, state, nameof(MtaStsPolicyFetched));

            _log.LogInformation($"Policy persisted for Domain: {domainName}");
        }

        public async Task Handle(DomainDeleted message)
        {
            string domainName = message.Id.ToLower();
            
            await _dao.Delete(domainName);
            _log.LogInformation($"Deleted MTA-STS entity with id: {domainName}.");
            
            DeleteScheduledReminder deleteScheduledReminder = new DeleteScheduledReminder(
                Guid.NewGuid().ToString(),
                ServiceName,
                domainName);
            
            _dispatcher.Dispatch(deleteScheduledReminder, _mtaStsEntityConfig.SnsTopicArn);
            _log.LogInformation(
                $"A DeleteScheduledReminder message for Domain: {domainName} has been dispatched to SnsTopic: {_mtaStsEntityConfig.SnsTopicArn}");
        }

        public async Task Handle(MtaStsRecordsEvaluated message)
        {
            string domainName = message.Id.ToLower();

            MtaStsEntityState state = await LoadState(domainName, nameof(message));
            _advisoryChangesNotifier.Handle(domainName, state.Messages, message.AdvisoryMessages);

            _log.LogInformation($"Updating MtaStsEntity.MtaStsState from {state.MtaStsState} to {MtaStsState.Evaluated} for {domainName}.");

            state.MtaStsRecords = message.Records;
            state.MtaStsState = MtaStsState.Evaluated;
            state.Version++;
            state.LastUpdated = message.LastUpdated;
            state.Messages = message.AdvisoryMessages;

            await _dao.Upsert(state);

            _entityChangedPublisher.Publish(domainName, state, nameof(MtaStsRecordsEvaluated));
            
            ReminderSuccessful reminderSuccessful = new ReminderSuccessful(
                Guid.NewGuid().ToString(),
                ServiceName,
                domainName,
                _clock.GetDateTimeUtc());
            
            _dispatcher.Dispatch(reminderSuccessful, _mtaStsEntityConfig.SnsTopicArn);
            _log.LogInformation(
                $"A ReminderSuccessful message for Domain: {domainName} has been dispatched to SnsTopic: {_mtaStsEntityConfig.SnsTopicArn}");
        }

        private async Task<MtaStsEntityState> LoadState(string domainName, string messageType)
        {
            MtaStsEntityState state = await _dao.Get(domainName);

            if (state == null)
            {
                _log.LogError($"Ignoring {messageType} as MtaSts Entity does not exist for {domainName}.");
                throw new MailCheckException(
                    $"Cannot handle event {messageType} as MtaSts Entity does not exist for {domainName}.");
            }

            return state;
        }
    }
}
