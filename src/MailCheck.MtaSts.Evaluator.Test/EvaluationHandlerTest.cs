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

namespace MailCheck.MtaSts.Evaluator.Test
{
    [TestFixture]
    public class EvaluationHandlerTest
    {
        private EvaluationHandler _evaluationHandler;
        private IMtaStsEvaluationProcessor _mtaStsEvaluationProcessor;
        private IMessageDispatcher _messageDispatcher;
        private IMtaStsEvaluatorConfig _mtaStsEvaluatorConfig;

        [SetUp]
        public void SetUp()
        {
            _mtaStsEvaluationProcessor = A.Fake<IMtaStsEvaluationProcessor>();
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _mtaStsEvaluatorConfig = A.Fake<IMtaStsEvaluatorConfig>();
            _evaluationHandler = new EvaluationHandler(_mtaStsEvaluationProcessor, _messageDispatcher, _mtaStsEvaluatorConfig);
        }

        [Test]
        public async Task HandleShouldProcessRecordsAndDispatchResult()
        {
            MtaStsRecords mtaStsRecords = new MtaStsRecords(null, null, 0);
            MtaStsRecordsPolled mtaStsRecordsPolled = new MtaStsRecordsPolled(null, null, mtaStsRecords, null);

            AdvisoryMessage messageFromProcessor = new AdvisoryMessage(Guid.Empty, AdvisoryType.Error, null, null);
            List<AdvisoryMessage> messagesFromProcessor = new List<AdvisoryMessage> { messageFromProcessor };
            A.CallTo(() => _mtaStsEvaluationProcessor.Process(mtaStsRecords)).Returns(messagesFromProcessor);
            A.CallTo(() => _mtaStsEvaluatorConfig.SnsTopicArn).Returns("testSnsTopicArn");

            await _evaluationHandler.Handle(mtaStsRecordsPolled);

            Expression<Func<MtaStsRecordsEvaluated, bool>> expected = x => x.Records == mtaStsRecords && x.AdvisoryMessages.Contains(messageFromProcessor);

            A.CallTo(() => _messageDispatcher.Dispatch(A<MtaStsRecordsEvaluated>.That.Matches(expected), "testSnsTopicArn")).MustHaveHappenedOnceExactly();
        }
    }
}
