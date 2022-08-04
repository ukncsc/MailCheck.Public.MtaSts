using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Contracts;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Evaluator.Config;
using NUnit.Framework;
using MailCheck.MtaSts.Evaluator.Explainers;

namespace MailCheck.MtaSts.Evaluator.Test
{
    [TestFixture]
    public class EvaluationHandlerTest
    {
        private EvaluationHandler _evaluationHandler;
        private IMtaStsRecordExplainer _mtaStsExplainer;
        private IMessageDispatcher _messageDispatcher;
        private IMtaStsEvaluatorConfig _mtaStsEvaluatorConfig;

        [SetUp]
        public void SetUp()
        {
            _mtaStsExplainer = A.Fake<IMtaStsRecordExplainer>();
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _mtaStsEvaluatorConfig = A.Fake<IMtaStsEvaluatorConfig>();
            _evaluationHandler = new EvaluationHandler(_mtaStsExplainer, _messageDispatcher, _mtaStsEvaluatorConfig);
        }

        [Test]
        public async Task HandleShouldProcessRecordsAndDispatchResult()
        {
            string domainName = "domainName";

            var record = new MtaStsRecord(domainName, new List<string> { "record" }, null);
            List<MtaStsRecord> records = new List<MtaStsRecord>
            {
                record
            };
            MtaStsRecords mtaStsRecords = new MtaStsRecords(null, records, 0);
            var pollerAdvisories = new List<MtaStsAdvisoryMessage>();
            MtaStsRecordsPolled mtaStsRecordsPolled = new MtaStsRecordsPolled(domainName, null, mtaStsRecords, pollerAdvisories);

            A.CallTo(() => _mtaStsEvaluatorConfig.SnsTopicArn).Returns("testSnsTopicArn");

            await _evaluationHandler.Handle(mtaStsRecordsPolled);

            Expression<Func<MtaStsRecordsEvaluated, bool>> expected = x => x.Records == mtaStsRecords && x.AdvisoryMessages == pollerAdvisories;
            
            A.CallTo(() => _mtaStsExplainer.Process(record)).MustHaveHappened();
            A.CallTo(() => _messageDispatcher.Dispatch(A<MtaStsRecordsEvaluated>.That.Matches(expected), "testSnsTopicArn")).MustHaveHappenedOnceExactly();
        }
    }
}
