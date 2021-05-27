using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using FakeItEasy;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Contracts.Messages;
using NUnit.Framework;

namespace MailCheck.MtaSts.Poller.Test.Integration
{
    public class RecordTests : PollHandlerTests
    {
        private PollHandler _pollHandler;

        [SetUp]
        public void SetUp()
        {
            _pollHandler = CreateSystemUnderTest();
        }

        [Test]
        public async Task InvalidCharactersInTagIsInvalid()
        {
            SetUpDnsClient("_mta-sts.testDomain.com", "v=STSv1; $$=@@");

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory1 = result.AdvisoryMessages[0];
            Assert.AreEqual(AdvisoryType.Error, advisory1.AdvisoryType);
            Assert.AreEqual("Please refer to SMTP MTA Strict Transport Security [RFC](https://tools.ietf.org/html/rfc8461#section-3.1)", advisory1.MarkDown);
            Assert.AreEqual("Invalid tag key", advisory1.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory1.MessageDisplay);

            Assert.AreEqual(1, result.MtaStsRecords.Records.Count);

            MtaStsRecord record = result.MtaStsRecords.Records[0];
            Assert.AreEqual("testDomain.com", record.Domain);
            Assert.AreEqual(2, record.Tags.Count);
        }


        [Test]
        public async Task MalformedTagIsInvalid()
        {
            SetUpDnsClient("_mta-sts.testDomain.com", "v=STSv1; id=123v=STSv1");

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory = result.AdvisoryMessages[0];
            Assert.AreEqual(AdvisoryType.Error, advisory.AdvisoryType);
            Assert.AreEqual(null, advisory.MarkDown);
            Assert.AreEqual("Malformed tag id=123v=STSv1", advisory.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory.MessageDisplay);

            Assert.AreEqual(1, result.MtaStsRecords.Records.Count);

            MtaStsRecord record = result.MtaStsRecords.Records[0];
            Assert.AreEqual("testDomain.com", record.Domain);
            Assert.AreEqual(2, record.Tags.Count);
        }

        [Test]
        public async Task RepeatedTagIsInvalid()
        {
            SetUpDnsClient("_mta-sts.testDomain.com", "v=STSv1; v=STSv1");

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory = result.AdvisoryMessages[0];
            Assert.AreEqual(AdvisoryType.Error, advisory.AdvisoryType);
            Assert.AreEqual(null, advisory.MarkDown);
            Assert.AreEqual("The v tagKey should occur no more than 1. This record has at least 1 occurrences.", advisory.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory.MessageDisplay);

            Assert.AreEqual(1, result.MtaStsRecords.Records.Count);

            MtaStsRecord record = result.MtaStsRecords.Records[0];
            Assert.AreEqual("testDomain.com", record.Domain);
            Assert.AreEqual(2, record.Tags.Count);
        }

        [Test]
        public async Task EmptyRecord()
        {
            string expectedMarkDown =
                "Mail Transfer Agent Strict Transport Security (MTA-STS) is a protocol that allows a domain to advertise the capability to receive emails using Transport Layer Security (TLS) 1.2 or higher. "
                + "This then allows a sending email service to safely enforce encryption, without the risk of losing emails."
                + $"{Environment.NewLine}[Further information about MTA-STS](https://www.mailcheck.service.ncsc.gov.uk/app/help/mtasts)";

            SetUpDnsClient("_mta-sts.testDomain.com", "");

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled) message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory = result.AdvisoryMessages[0];
            Assert.AreEqual(AdvisoryType.Warning, advisory.AdvisoryType);
            Assert.AreEqual(expectedMarkDown, advisory.MarkDown);
            Assert.AreEqual("No MTA-STS record configured.", advisory.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory.MessageDisplay);

            Assert.AreEqual(0, result.MtaStsRecords.Records.Count);
        }

        [Test]
        public async Task MultipleRecordsIsInvalid()
        {
            List<DnsResourceRecord> records = new List<DnsResourceRecord>
            {
                new TxtRecord(new ResourceRecordInfo("testDomain.com", ResourceRecordType.TXT, QueryClass.IN, 100, 100), new[] {"v=STSv1; id=123"}, new[] {"v=STSv1; id=123"}),
                new TxtRecord(new ResourceRecordInfo("testDomain.com", ResourceRecordType.TXT, QueryClass.IN, 100, 100), new[] {"v=STSv1; id=345"}, new[] {"v=STSv1; id=345"}),
            };

            IDnsQueryResponse dnsQueryResponse = new TestDnsQueryResponse(records);

            A.CallTo(() => LookupClient.QueryAsync("_mta-sts.testDomain.com", QueryType.TXT, A<QueryClass>._, A<CancellationToken>._)).Returns(dnsQueryResponse);

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory = result.AdvisoryMessages[0];
            Assert.AreEqual(AdvisoryType.Error, advisory.AdvisoryType);
            Assert.AreEqual(null, advisory.MarkDown);
            Assert.AreEqual("A domain should have only 1 MTA-STS record.", advisory.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory.MessageDisplay);
        }
    }
}