using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Contracts;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Contracts.Tags;
using NUnit.Framework;

namespace MailCheck.MtaSts.Poller.Test.Integration
{
    public class PolicyVersionIdTagTests : PollHandlerTests
    {
        private PollHandler _pollHandler;

        [SetUp]
        public void SetUp()
        {
            _pollHandler = CreateSystemUnderTest();
        }

        [Test]
        public async Task VersionAndIdIsValid()
        {
            SetUpDnsClient("_mta-sts.testDomain.com", "v=STSv1; id=123");

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(0, result.AdvisoryMessages.Count);
            Assert.AreEqual(1, result.MtaStsRecords.Records.Count);

            MtaStsRecord record = result.MtaStsRecords.Records[0];
            Assert.AreEqual("testDomain.com", record.Domain);
            Assert.AreEqual(2, record.Tags.Count);

            VersionTag versionTag = (VersionTag)record.Tags[0];
            Assert.AreEqual("VersionTag", versionTag.Type);
            Assert.AreEqual("v=STSv1", versionTag.RawValue);
            Assert.AreEqual("STSv1", versionTag.Value);

            PolicyVersionIdTag policyVersionIdTag = (PolicyVersionIdTag)record.Tags[1];
            Assert.AreEqual("PolicyVersionIdTag", policyVersionIdTag.Type);
            Assert.AreEqual("id=123", policyVersionIdTag.RawValue);
            Assert.AreEqual("123", policyVersionIdTag.Value);
        }

        [TestCase("")]
        [TestCase("*")]
        [TestCase("123456789012345678901234567890123")]
        public async Task InvalidPolicyVersionId(string badId)
        {
            SetUpDnsClient("_mta-sts.testDomain.com", $"v=STSv1; id={badId}");

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory = result.AdvisoryMessages[0];
            Assert.AreEqual(MessageType.error, advisory.MessageType);
            Assert.AreEqual(null, advisory.MarkDown);
            Assert.AreEqual("Invalid id. The value must be between 1 and 32 alphanumeric characters", advisory.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory.MessageDisplay);

            Assert.AreEqual(1, result.MtaStsRecords.Records.Count);

            MtaStsRecord record = result.MtaStsRecords.Records[0];
            Assert.AreEqual("testDomain.com", record.Domain);
            Assert.AreEqual(2, record.Tags.Count);
        }

        [Test]
        public async Task InvalidPolicyVersionMissingSemicolon()
        {
            SetUpDnsClient("_mta-sts.testDomain.com", "v=STSv1 id=021120211155");

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(2, result.AdvisoryMessages.Count);

            Assert.AreEqual("mailcheck.mtasts.malformedTag", result.AdvisoryMessages[0].Name);
            Assert.AreEqual("mailcheck.mtasts.versionTagRequired", result.AdvisoryMessages[1].Name);

            Assert.AreEqual(1, result.MtaStsRecords.Records.Count);

            MtaStsRecord record = result.MtaStsRecords.Records[0];
            Assert.AreEqual("testDomain.com", record.Domain);
            Assert.AreEqual(1, record.Tags.Count);
        }

    }
}