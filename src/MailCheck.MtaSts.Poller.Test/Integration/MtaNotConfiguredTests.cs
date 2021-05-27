using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using FakeItEasy;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Contracts.Messages;
using NUnit.Framework;

namespace MailCheck.MtaSts.Poller.Test.Integration
{
    public class MtaNotConfiguredTests : PollHandlerTests
    {
        private PollHandler _pollHandler;

        [SetUp]
        public void SetUp()
        {
            _pollHandler = CreateSystemUnderTest();
        }

        [Test]
        public async Task EmptyRecordGivesInfo()
        {
            string expectedMarkdown = "Mail Transfer Agent Strict Transport Security (MTA-STS) is a protocol that allows a domain to advertise the capability to receive emails using Transport Layer Security (TLS) 1.2 or higher. "
                                      + "This then allows a sending email service to safely enforce encryption, without the risk of losing emails."
                                      + $"{Environment.NewLine}[Further information about MTA-STS](https://www.mailcheck.service.ncsc.gov.uk/app/help/mtasts)";

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory = result.AdvisoryMessages[0];
            Assert.AreEqual(AdvisoryType.Warning, advisory.AdvisoryType);
            Assert.AreEqual(expectedMarkdown, advisory.MarkDown);
            Assert.AreEqual("No MTA-STS record configured.", advisory.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory.MessageDisplay);
        }

        [TestCase("Non-Existent Domain")]
        [TestCase("Server Failure")]
        public async Task ServerFailureAndNonExistentDomainTreatedAsNotConfigured(string errorMessage)
        {
            string expectedMarkdown =
                "Mail Transfer Agent Strict Transport Security (MTA-STS) is a protocol that allows a domain to advertise the capability to receive emails using Transport Layer Security (TLS) 1.2 or higher. "
                + "This then allows a sending email service to safely enforce encryption, without the risk of losing emails."
                + $"{Environment.NewLine}[Further information about MTA-STS](https://www.mailcheck.service.ncsc.gov.uk/app/help/mtasts)";

            TestDnsQueryResponse dnsQueryResponse = new TestDnsQueryResponse(new List<DnsResourceRecord>()) { HasError = true, ErrorMessage = errorMessage };
            A.CallTo(() => LookupClient.QueryAsync(A<string>._, QueryType.TXT, A<QueryClass>._, A<CancellationToken>._)).Returns(dnsQueryResponse);

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory = result.AdvisoryMessages[0];
            Assert.AreEqual(AdvisoryType.Warning, advisory.AdvisoryType);
            Assert.AreEqual(expectedMarkdown, advisory.MarkDown);
            Assert.AreEqual("No MTA-STS record configured.", advisory.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory.MessageDisplay);
        }
    }
}