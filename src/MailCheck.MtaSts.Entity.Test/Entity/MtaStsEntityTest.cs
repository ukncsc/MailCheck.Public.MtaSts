using System;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Common.Exception;
using MailCheck.MtaSts.Contracts;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Contracts.Entity;
using MailCheck.MtaSts.Contracts.External;
using MailCheck.MtaSts.Contracts.Tags;
using MailCheck.MtaSts.Entity.Config;
using MailCheck.MtaSts.Entity.Dao;
using MailCheck.Common.Util;
using Microsoft.Extensions.Logging;
using MailCheck.Common.Contracts.Messaging;
using NUnit.Framework;
using MailCheck.MtaSts.Entity.Entity;
using System.Collections.Generic;
using MailCheck.MtaSts.Entity.Entity.EmailSecurity;
using MailCheck.MtaSts.Entity.Entity.Notifiers;
using System.Linq.Expressions;
using MailCheck.MtaSts.Contracts.PolicyFetcher;
using System.Collections;

namespace MailCheck.MtaSts.Entity.Test.Entity
{
    [TestFixture]
    public class MtaStsEntityTest
    {
        private const string Id = "abc.com";

        private IMtaStsEntityDao _mtaStsEntityDao;
        private IMtaStsEntityConfig _mtaStsEntityConfig;
        private IMessageDispatcher _dispatcher;
        private IClock _clock;
        private ILogger<MtaStsEntity> _log;
        private MtaStsEntity _mtaStsEntity;
        private IEntityChangedPublisher _entityChangedPublisher;
        private IChangeNotifiersComposite _changeNotifiersComposite;

        [SetUp]
        public void SetUp()
        {
            _mtaStsEntityDao = A.Fake<IMtaStsEntityDao>();
            _mtaStsEntityConfig = A.Fake<IMtaStsEntityConfig>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _clock = A.Fake<IClock>();
            _log = A.Fake<ILogger<MtaStsEntity>>();
            _entityChangedPublisher = A.Fake<IEntityChangedPublisher>();
            _changeNotifiersComposite = A.Fake<IChangeNotifiersComposite>();
            _mtaStsEntity = new MtaStsEntity(_mtaStsEntityDao, _mtaStsEntityConfig, _dispatcher, _clock, _log,
                 _entityChangedPublisher, _changeNotifiersComposite);
        }

        [Test]
        public async Task HandlingDomainCreatedCreatesDomainIfEntityDoesNotExist()
        {
            A.CallTo(() => _mtaStsEntityDao.Get(Id)).Returns<MtaStsEntityState>(null);
            await _mtaStsEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));

            A.CallTo(() => _mtaStsEntityDao.Upsert(A<MtaStsEntityState>.That.Matches(_ =>
                _.Id == Id && _.MtaStsState == MtaStsState.Created && _.Version == 1))).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<MtaStsPollPending>._, A<string>._)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public async Task HandlingDomainCreatedDispatchesCreateScheduledReminder()
        {
            A.CallTo(() => _mtaStsEntityDao.Get(Id)).Returns<MtaStsEntityState>(null);
            await _mtaStsEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));

            A.CallTo(() => _dispatcher.Dispatch(A<CreateScheduledReminder>.That.Matches(_ => 
                _.ResourceId == "abc.com" && _.Service == "MtaSts" && _.ScheduledTime == default), A<string>._)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public void HandlingDomainCreatedDoesntDispatchCreateScheduledReminderIfEntityAlreadyExists()
        {
            A.CallTo(() => _mtaStsEntityDao.Get(Id))
                .Returns(new MtaStsEntityState(Id, 1, MtaStsState.PollPending, DateTime.UtcNow));

            A.CallTo(() => _dispatcher.Dispatch(A<CreateScheduledReminder>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandlingScheduledReminderDispatchesPollPending()
        {
            A.CallTo(() => _mtaStsEntityDao.Get(Id))
                .Returns(new MtaStsEntityState(Id, 1, MtaStsState.Evaluated, DateTime.UtcNow));
            await _mtaStsEntity.Handle(new MtaStsScheduledReminder("1234", Id));

            A.CallTo(() => _mtaStsEntityDao.Upsert(A<MtaStsEntityState>.That.Matches(_ =>
                    _.Id == Id && _.MtaStsState == MtaStsState.PollPending && _.Version == 2)))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<MtaStsPollPending>.That.Matches(_ => _.Id == Id), A<string>._))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandlingDomainDeletedDeletesStateFromDb()
        {
            await _mtaStsEntity.Handle(new DomainDeleted(Id));

            A.CallTo(() => _mtaStsEntityDao.Delete(Id)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public async Task HandlingDomainDeletedDispatchesDeleteScheduledReminder()
        {
            await _mtaStsEntity.Handle(new DomainDeleted(Id));

            A.CallTo(() => _dispatcher.Dispatch(A<DeleteScheduledReminder>.That.Matches(_ => 
                _.ResourceId == Id && _.Service == "MtaSts"), A<string>._)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public async Task Handle_MtaStsRecordsEvaluatedWithRecords_UpdatesStateAndTriggersPolicyFetch()
        {
            MtaStsRecords mtaStsRecords = CreateMtaStsRecords();

            var policyAdvisory = A.Fake<MtaStsAdvisoryMessage>(options => options.Named("policyAdvisory").CallsBaseMethods());
            var recordAdvisory = A.Fake<MtaStsAdvisoryMessage>(options => options.Named("recordAdvisory").CallsBaseMethods());
            var existingPolicy = new MtaStsPolicyResult("raw policy", new List<Contracts.Keys.Key>(), new List<MtaStsAdvisoryMessage> { policyAdvisory });
            var existingState = new MtaStsEntityState(Id, 1, MtaStsState.PollPending, DateTime.UtcNow)
            {
                Policy = existingPolicy,
                Messages = new List<MtaStsAdvisoryMessage> { recordAdvisory },
            };
            A.CallTo(() => _mtaStsEntityDao.Get(Id)).Returns(existingState);

            MtaStsRecordsEvaluated message =
                new MtaStsRecordsEvaluated(Id, mtaStsRecords, new List<MtaStsAdvisoryMessage>(), DateTime.UtcNow);

            await _mtaStsEntity.Handle(message);

            Expression<Func<MtaStsEntityState, bool>> predicate = state => 
                state.Id == Id && 
                state.MtaStsState == MtaStsState.Evaluated &&
                state.MtaStsRecords == message.Records && 
                state.Version == 2 &&
                state.LastUpdated == message.LastUpdated;
            
            A.CallTo(() => _mtaStsEntityDao.Upsert(A<MtaStsEntityState>.That.Matches(predicate))).MustHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<ReminderSuccessful>._, A<string>._)).MustHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<MtaStsFetchPolicy>._, A<string>._)).MustHaveHappened();
            A.CallTo(() => _entityChangedPublisher.Publish(Id, existingState, A<string>._)).MustNotHaveHappened();

            A.CallTo(() => _changeNotifiersComposite.Handle(A<string>._, A<IEnumerable<AdvisoryMessage>>._, A<IEnumerable<AdvisoryMessage>>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task Handle_MtaStsRecordsEvaluatedWithNoRecords_UpdatesStateAndClearsPolicy()
        {
            MtaStsRecords mtaStsRecords = CreateEmptyMtaStsRecords();

            var policyAdvisory = A.Fake<MtaStsAdvisoryMessage>(options => options.Named("policyAdvisory").CallsBaseMethods());
            var recordAdvisory = A.Fake<MtaStsAdvisoryMessage>(options => options.Named("recordAdvisory").CallsBaseMethods());
            var existingPolicy = new MtaStsPolicyResult("raw policy", new List<Contracts.Keys.Key>(), new List<MtaStsAdvisoryMessage> { policyAdvisory });
            var existingState = new MtaStsEntityState(Id, 1, MtaStsState.PollPending, DateTime.UtcNow)
            {
                Policy = existingPolicy,
                Messages = new List<MtaStsAdvisoryMessage> { recordAdvisory },
            };
            A.CallTo(() => _mtaStsEntityDao.Get(Id)).Returns(existingState);

            MtaStsRecordsEvaluated message =
                new MtaStsRecordsEvaluated(Id, mtaStsRecords, new List<MtaStsAdvisoryMessage>(), DateTime.UtcNow);

            await _mtaStsEntity.Handle(message);

            Expression<Func<MtaStsEntityState, bool>> predicate = state =>
                state.Id == Id &&
                state.MtaStsState == MtaStsState.Evaluated &&
                state.MtaStsRecords == message.Records &&
                state.Version == 2 &&
                state.LastUpdated == message.LastUpdated && 
                state.Policy.RawValue == null &&
                state.Policy.Errors.Count == 0;

            A.CallTo(() => _mtaStsEntityDao.Upsert(A<MtaStsEntityState>.That.Matches(predicate))).MustHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<ReminderSuccessful>._, A<string>._)).MustHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<MtaStsFetchPolicy>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _entityChangedPublisher.Publish(Id, existingState, A<string>._)).MustHaveHappened();

            A.CallTo(() => _changeNotifiersComposite.Handle(Id, 
                A<IEnumerable<AdvisoryMessage>>.That.IsSameSequenceAs(new[] { recordAdvisory, policyAdvisory }),
                A<IEnumerable<AdvisoryMessage>>.That.IsEmpty()
            )).MustHaveHappenedOnceExactly();
        }


        [Test]
        public async Task Handle_MtaStsPolicyFetched_UpdatesStateAndPublishedMessages()
        {
            MtaStsRecords mtaStsRecords = CreateMtaStsRecords();

            var policyAdvisory = A.Fake<MtaStsAdvisoryMessage>(options => options.Named("policyAdvisory").CallsBaseMethods());
            var recordAdvisory = A.Fake<MtaStsAdvisoryMessage>(options => options.Named("recordAdvisory").CallsBaseMethods());

            var lastUpdated = new DateTime(1, 2, 3, 4, 5, 6, 7);
            var existingState = new MtaStsEntityState(Id, 1, MtaStsState.PollPending, DateTime.UtcNow)
            {
                MtaStsRecords = mtaStsRecords,
                Messages = new List<MtaStsAdvisoryMessage> { recordAdvisory },
                LastUpdated = lastUpdated
            };

            A.CallTo(() => _mtaStsEntityDao.Get(Id)).Returns(existingState);

            var policyResult = new MtaStsPolicyResult("raw policy", new List<Contracts.Keys.Key>(), new List<MtaStsAdvisoryMessage> { policyAdvisory });

            MtaStsPolicyFetched message = new MtaStsPolicyFetched(Id, policyResult);

            await _mtaStsEntity.Handle(message);

            Expression<Func<MtaStsEntityState, bool>> predicate = state =>
                state.Id == Id &&
                state.MtaStsState == MtaStsState.PollPending &&
                state.MtaStsRecords == mtaStsRecords &&
                state.Version == 2 &&
                state.Policy == policyResult &&
                state.LastUpdated == lastUpdated;

            A.CallTo(() => _mtaStsEntityDao.Upsert(A<MtaStsEntityState>.That.Matches(predicate))).MustHaveHappened();
            A.CallTo(() => _entityChangedPublisher.Publish(Id, existingState, A<string>._)).MustHaveHappened();
            A.CallTo(() => _changeNotifiersComposite.Handle(Id, 
                A<IEnumerable<AdvisoryMessage>>.That.IsSameSequenceAs(recordAdvisory), 
                A<IEnumerable<AdvisoryMessage>>.That.IsSameSequenceAs(recordAdvisory, policyAdvisory)
            )).MustHaveHappenedOnceExactly();
        }

        private static MtaStsRecords CreateMtaStsRecords(string domain = Id)
        {
            List<string> recordParts = new List<string> { "v = STSv1;", "id = 123456" };
            MtaStsRecord record = new MtaStsRecord(domain, recordParts, new List<Tag>());
            MtaStsRecords records = new MtaStsRecords(domain, new List<MtaStsRecord> { record }, 100);
            return records;
        }

        private static MtaStsRecords CreateEmptyMtaStsRecords(string domain = Id)
        {
            MtaStsRecords records = new MtaStsRecords(domain, new List<MtaStsRecord> { }, 0);
            return records;
        }
    }
}