using System.Collections.Generic;
using System.Threading;
using Amazon.SimpleNotificationService;
using DnsClient;
using DnsClient.Protocol;
using FakeItEasy;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.MtaSts.Poller.Test.Integration
{
    public class PollHandlerTests
    {
        protected ILookupClient LookupClient;
        protected IMessageDispatcher MessageDispatcher;

        public PollHandler CreateSystemUnderTest()
        {
            ServiceCollection fakeServices = new ServiceCollection();

            LookupClient = A.Fake<ILookupClient>();
            fakeServices.AddTransient(_ => LookupClient);
            MessageDispatcher = A.Fake<IMessageDispatcher>();
            fakeServices.AddTransient(_ => MessageDispatcher);
            fakeServices.AddTransient(_ => A.Fake<IAmazonSimpleNotificationService>());
            fakeServices.AddTransient(_ => A.Fake<IEnvironmentVariables>());

            IntegrationTestStartUp startUp = new IntegrationTestStartUp(fakeServices);

            ServiceCollection services = new ServiceCollection();
            startUp.ConfigureServices(services);

            return services.BuildServiceProvider().GetRequiredService<PollHandler>();
        }

        protected void SetUpDnsClient(string query, string recordString)
        {
            List<DnsResourceRecord> records = new List<DnsResourceRecord>
            {
                new TxtRecord(new ResourceRecordInfo("testDomain.com", ResourceRecordType.TXT, QueryClass.IN, 100, 100), new[] {recordString}, new[] {recordString})
            };

            IDnsQueryResponse dnsQueryResponse =  new TestDnsQueryResponse(records);

            A.CallTo(() => LookupClient.QueryAsync(query, QueryType.TXT, A<QueryClass>._, A<CancellationToken>._)).Returns(dnsQueryResponse);
        }
    }
}