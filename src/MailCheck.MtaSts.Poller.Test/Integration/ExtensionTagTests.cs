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
    public class ExtensionTagTests : PollHandlerTests
    {
        private PollHandler _pollHandler;

        [SetUp]
        public void SetUp()
        {
            _pollHandler = CreateSystemUnderTest();
        }

        [Test]
        public async Task VersionAndExtensionIsValid()
        {
            string validValues = "!\"#$%&'()*+,-./0123456789:<>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            SetUpDnsClient("_mta-sts.testDomain.com", $"v=STSv1; extension-1_2.3={validValues};");

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

            ExtensionTag extensionTag = (ExtensionTag)record.Tags[1];
            Assert.AreEqual("ExtensionTag", extensionTag.Type);
            Assert.AreEqual($"extension-1_2.3={validValues}", extensionTag.RawValue);
            Assert.AreEqual(validValues, extensionTag.Value);
        }

        [TestCase("*")]
        [TestCase("123456789012345678901234567890123")]
        public async Task InvalidExtensionKey(string key)
        {
            SetUpDnsClient("_mta-sts.testDomain.com", $"v=STSv1; {key}=value");

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory = result.AdvisoryMessages[0];
            Assert.AreEqual(MessageType.error, advisory.MessageType);
            Assert.AreEqual("Please refer to SMTP MTA Strict Transport Security [RFC](https://tools.ietf.org/html/rfc8461#section-3.1)", advisory.MarkDown);
            Assert.AreEqual("Invalid tag key", advisory.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory.MessageDisplay);

            Assert.AreEqual(1, result.MtaStsRecords.Records.Count);

            MtaStsRecord record = result.MtaStsRecords.Records[0];
            Assert.AreEqual("testDomain.com", record.Domain);
            Assert.AreEqual(2, record.Tags.Count);
        }

        [Test]
        public async Task InvalidExtensionValue()
        {
            SetUpDnsClient("_mta-sts.testDomain.com", "v=STSv1; key=£;");

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory = result.AdvisoryMessages[0];
            Assert.AreEqual(MessageType.error, advisory.MessageType);
            Assert.AreEqual("Please refer to SMTP MTA Strict Transport Security [RFC](https://tools.ietf.org/html/rfc8461#section-3.1)", advisory.MarkDown);
            Assert.AreEqual("Invalid tag value", advisory.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory.MessageDisplay);

            Assert.AreEqual(1, result.MtaStsRecords.Records.Count);

            MtaStsRecord record = result.MtaStsRecords.Records[0];
            Assert.AreEqual("testDomain.com", record.Domain);
            Assert.AreEqual(2, record.Tags.Count);
        }
    }
}