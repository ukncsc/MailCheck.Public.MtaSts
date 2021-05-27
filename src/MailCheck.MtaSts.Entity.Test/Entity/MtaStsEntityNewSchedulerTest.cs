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

namespace MailCheck.MtaSts.Entity.Test.Entity
{
    [TestFixture]
    public class MtaStsEntityNewSchedulerTest
    {
        private const string Id = "abc.com";

        private IMtaStsEntityDao _mtaStsEntityDao;
        private IMtaStsEntityConfig _mtaStsEntityConfig;
        private IMessageDispatcher _dispatcher;
        private IClock _clock;
        private ILogger<MtaStsEntityNewScheduler> _log;
        private MtaStsEntityNewScheduler _mtaStsEntity;
        private IEntityChangedPublisher _entityChangedPublisher;
        private IChangeNotifier _advisoryChangesNotifier;

        [SetUp]
        public void SetUp()
        {
            _mtaStsEntityDao = A.Fake<IMtaStsEntityDao>();
            _mtaStsEntityConfig = A.Fake<IMtaStsEntityConfig>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _clock = A.Fake<IClock>();
            _log = A.Fake<ILogger<MtaStsEntityNewScheduler>>();
            _entityChangedPublisher = A.Fake<IEntityChangedPublisher>();
            _advisoryChangesNotifier = A.Fake<IChangeNotifier>();
            _mtaStsEntity = new MtaStsEntityNewScheduler(_mtaStsEntityDao, _mtaStsEntityConfig, _dispatcher, _clock, _log,
                 _entityChangedPublisher, _advisoryChangesNotifier);
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
        public void HandlingDomainCreatedThrowsIfEntityAlreadyExistsForDomain()
        {
            A.CallTo(() => _mtaStsEntityDao.Get(Id))
                .Returns(new MtaStsEntityState(Id, 1, MtaStsState.PollPending, DateTime.UtcNow));
            Assert.ThrowsAsync<MailCheckException>(() =>
                _mtaStsEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now)));

            A.CallTo(() => _mtaStsEntityDao.Upsert(A<MtaStsEntityState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<MtaStsPollPending>.That.Matches(_ => _.Id == Id), A<string>._))
                .MustNotHaveHappened();
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
        public async Task HandlingRecordsEvaluatedUpdatesStateWithNoCreateScheduledReminder()
        {
            Environment.SetEnvironmentVariable("ACTIVE_FEATURES", "NewScheduler");
            MtaStsRecords mtaStsRecords = CreateMtaStsRecords();

            A.CallTo(() => _mtaStsEntityDao.Get(Id))
                .Returns(new MtaStsEntityState(Id, 1, MtaStsState.PollPending, DateTime.UtcNow));

            MtaStsRecordsEvaluated message =
                new MtaStsRecordsEvaluated(Id, mtaStsRecords, new List<AdvisoryMessage>(), DateTime.UtcNow);
            await _mtaStsEntity.Handle(message);

            A.CallTo(() => _mtaStsEntityDao.Upsert(A<MtaStsEntityState>.That.Matches(_ => _.Id == Id
                                                                                          && _.MtaStsState ==
                                                                                          MtaStsState.Evaluated &&
                                                                                          _.MtaStsRecords ==
                                                                                          message.Records
                                                                                          && _.Version == 2 &&
                                                                                          _.LastUpdated ==
                                                                                          message.LastUpdated)));
            A.CallTo(() =>
                _dispatcher.Dispatch(
                    A<CreateScheduledReminder>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() =>
                    _advisoryChangesNotifier.Handle(A<string>._, A<List<AdvisoryMessage>>._,
                        A<List<AdvisoryMessage>>._))
                .MustHaveHappenedOnceExactly();
        }

        private static MtaStsRecords CreateMtaStsRecords(string domain = Id)
        {
            List<string> recordParts = new List<string> { "v = STSv1;", "id = 123456" };
            MtaStsRecord record = new MtaStsRecord(domain, recordParts, new List<Tag>());
            MtaStsRecords records = new MtaStsRecords(domain, new List<MtaStsRecord> { record }, 100);
            return records;
        }
    }
}
