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
using NUnit.Framework;

namespace MailCheck.MtaSts.Poller.Test.Integration
{
    public class FailedPollTests : PollHandlerTests
    {
        private PollHandler _pollHandler;

        [SetUp]
        public void SetUp()
        {
            _pollHandler = CreateSystemUnderTest();
        }

        [Test]
        public async Task FailedPollGivesAdvisory()
        {
            TestDnsQueryResponse dnsQueryResponse = new TestDnsQueryResponse(new List<DnsResourceRecord>()) { HasError = true, ErrorMessage = "Error Message" };
            A.CallTo(() => LookupClient.QueryAsync(A<string>._, QueryType.TXT, A<QueryClass>._, A<CancellationToken>._)).Returns(dnsQueryResponse);

            MtaStsRecordsPolled result = null;
            A.CallTo(() => MessageDispatcher.Dispatch(A<Message>._, A<string>._))
                .Invokes((Message message, string topic) => { result = (MtaStsRecordsPolled)message; });

            await _pollHandler.Handle(new MtaStsPollPending("testDomain.com"));

            Assert.AreEqual(1, result.AdvisoryMessages.Count);

            AdvisoryMessage advisory = result.AdvisoryMessages[0];
            Assert.AreEqual(MessageType.error, advisory.MessageType);
            Assert.AreEqual(null, advisory.MarkDown);
            Assert.AreEqual("Error Message", advisory.Text);
            Assert.AreEqual(MessageDisplay.Standard, advisory.MessageDisplay);
        }
    }
}