using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Api.Config;
using MailCheck.MtaSts.Api.Dao;
using MailCheck.MtaSts.Api.Domain;
using MailCheck.MtaSts.Api.Service;
using MailCheck.MtaSts.Contracts.Entity;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.MtaSts.Api.Test.Service
{
    [TestFixture]
    public class MtaStsServiceTests
    {
        private MtaStsService _mtaStsService;
        private IMessagePublisher _messagePublisher;
        private IMtaStsApiDao _dao;
        private IMtaStsApiConfig _config;
        private ILogger<MtaStsService> _log;

        [SetUp]
        public void SetUp()
        {
            _messagePublisher = A.Fake<IMessagePublisher>();
            _dao = A.Fake<IMtaStsApiDao>();
            _config = A.Fake<IMtaStsApiConfig>();
            _log = A.Fake<ILogger<MtaStsService>>();

            _mtaStsService = new MtaStsService(_messagePublisher, _dao, _config, _log);
        }

        [Test]
        public async Task PublishesDomainMissingMessageWhenDomainDoesNotExist()
        {
            A.CallTo(() => _dao.GetMtaStsForDomain("testDomain"))
                .Returns(Task.FromResult<MtaStsInfoResponse>(null));

            MtaStsInfoResponse result = await _mtaStsService.GetMtaStsForDomain("testDomain");

            A.CallTo(() => _messagePublisher.Publish(A<DomainMissing>._, A<string>._))
                .MustHaveHappenedOnceExactly();
            Assert.AreEqual(null, result);
        }

        [Test]
        public async Task DoesNotPublishDomainMissingMessageWhenDomainExists()
        {
            MtaStsInfoResponse mtaStsInfoResponse = new MtaStsInfoResponse("", MtaStsState.Created);
            A.CallTo(() => _dao.GetMtaStsForDomain("testDomain"))
                .Returns(Task.FromResult(mtaStsInfoResponse));

            MtaStsInfoResponse result = await _mtaStsService.GetMtaStsForDomain("testDomain");

            A.CallTo(() => _messagePublisher.Publish(A<DomainMissing>._, A<string>._))
                .MustNotHaveHappened();
            Assert.AreSame(mtaStsInfoResponse, result);

        }
    }
}