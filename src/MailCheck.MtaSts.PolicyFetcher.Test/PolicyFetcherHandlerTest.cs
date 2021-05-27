using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Contracts;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Contracts.PolicyFetcher;
using MailCheck.MtaSts.PolicyFetcher.Config;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.MtaSts.PolicyFetcher.Test
{
    [TestFixture]
    public class PolicyFetcherHandlerTest
    {
        private PolicyFetcherHandler _policyFetcherHandler;
        private IPolicyFetcher _policyFetcher;
        private IMessageDispatcher _messageDispatcher;
        private IMtaStsPolicyFetcherConfig _mtaStsPolicyFetcherConfig;
        private ILogger<PolicyFetcherHandler> _log;

        [SetUp]
        public void SetUp()
        {
            _policyFetcher = A.Fake<IPolicyFetcher>();
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _mtaStsPolicyFetcherConfig = A.Fake<IMtaStsPolicyFetcherConfig>();
            _log = A.Fake<ILogger<PolicyFetcherHandler>>();
            _policyFetcherHandler = new PolicyFetcherHandler(_policyFetcher, _messageDispatcher, _mtaStsPolicyFetcherConfig, _log);
        }

        [Test]
        public async Task HandleShouldFetchParsedPolicyAndDispatchResult()
        {
            MtaStsFetchPolicy mtaStsFetchPolicy = new MtaStsFetchPolicy("ncsc.gov.uk");
            string policy = PolicyExample.ValidPolicy;
            MxKey mxKey = new MxKey("ncsc-gov-uk.mail.protection.outlook.com", "mx: ncsc-gov-uk.mail.protection.outlook.com");
            ModeKey modeKey = new ModeKey("enforce", "mode: enforce");
            MaxAgeKey maxAgeKey = new MaxAgeKey("86400", "max_age: 86400");
            VersionKey versionKey = new VersionKey("STSv1", "version: STSv1");

            List<Key> keys = new List<Key>() { mxKey, modeKey, maxAgeKey, versionKey };
            MtaStsPolicyResult result = new MtaStsPolicyResult(policy, keys, new List<AdvisoryMessage>());
            A.CallTo(() => _policyFetcher.Process("ncsc.gov.uk")).Returns(result);
            A.CallTo(() => _mtaStsPolicyFetcherConfig.SnsTopicArn).Returns("testSnsTopicArn");

            await _policyFetcherHandler.Handle(mtaStsFetchPolicy);

            Expression<Func<MtaStsPolicyFetched, bool>> expected = x => x.Id == "ncsc.gov.uk" &&
            x.MtaStsPolicyResult.RawValue == policy && x.MtaStsPolicyResult.Errors.Count == 0 && x.MtaStsPolicyResult.Keys.Count == 4;

            A.CallTo(() => _messageDispatcher.Dispatch(A<MtaStsPolicyFetched>.That.Matches(expected), "testSnsTopicArn")).MustHaveHappenedOnceExactly();
        }
    }
}
