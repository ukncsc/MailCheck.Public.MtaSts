using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Contracts;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Contracts.Tags;
using NUnit.Framework;

namespace MailCheck.MtaSts.Poller.Test.Integration
{
    public class VersionTagTests : PollHandlerTests
    {
        private PollHandler _pollHandler;

        [SetUp]
        public void SetUp()
        {
            _pollHandler = CreateSystemUnderTest();
        }

        [Test]
        public async Task VersionOnlyIsValid()
        {
            SetUpDnsClient("_mta-sts.testDomain.com", "v=STSv1");

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(0, result.AdvisoryMessages.Count);
            Assert.AreEqual(1, result.MtaStsRecords.Records.Count);

            MtaStsRecord record = result.MtaStsRecords.Records[0];
            Assert.AreEqual("testDomain.com", record.Domain);
            Assert.AreEqual(1, record.Tags.Count);

            VersionTag versionTag = (VersionTag)record.Tags[0];
            Assert.AreEqual("VersionTag", versionTag.Type);
            Assert.AreEqual("v=STSv1", versionTag.RawValue);
            Assert.AreEqual("STSv1", versionTag.Value);
        }

        [Test]
        public async Task InvalidVersion()
        {
            SetUpDnsClient("_mta-sts.testDomain.com", "v=STSv2");

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory = result.AdvisoryMessages[0];
            Assert.AreEqual(AdvisoryType.Error, advisory.AdvisoryType);
            Assert.AreEqual(null, advisory.MarkDown);
            Assert.AreEqual("Invalid version. MtaSts record must start with v=STSv1.", advisory.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory.MessageDisplay);

            Assert.AreEqual(1, result.MtaStsRecords.Records.Count);

            MtaStsRecord record = result.MtaStsRecords.Records[0];
            Assert.AreEqual("testDomain.com", record.Domain);
            Assert.AreEqual(1, record.Tags.Count);
        }

        [Test]
        public async Task IncorrectCaseVersion()
        {
            SetUpDnsClient("_mta-sts.testDomain.com", "v=stsv1");

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory = result.AdvisoryMessages[0];
            Assert.AreEqual(AdvisoryType.Error, advisory.AdvisoryType);
            Assert.AreEqual(null, advisory.MarkDown);
            Assert.AreEqual("Invalid version. MtaSts record must start with v=STSv1.", advisory.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory.MessageDisplay);

            Assert.AreEqual(1, result.MtaStsRecords.Records.Count);

            MtaStsRecord record = result.MtaStsRecords.Records[0];
            Assert.AreEqual("testDomain.com", record.Domain);
            Assert.AreEqual(1, record.Tags.Count);
        }
    }
}