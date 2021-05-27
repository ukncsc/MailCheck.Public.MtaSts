using System;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Entity.Config;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.MtaSts.Entity.Entity.Notifications;
using MailCheck.MtaSts.Entity.Entity.Notifiers;
using AdvisoryMessage = MailCheck.Common.Contracts.Advisories.AdvisoryMessage;

namespace MailCheck.MtaSts.Entity.Test.Entity.Notifiers
{

    public class AdvisoryChangedNotifierTestCase
    {
        public List<AdvisoryMessage> CurrentMessages { get; set; }
        public List<AdvisoryMessage> NewMessages { get; set; }
        public int ExpectedAdded { get; set; }
        public int ExpectedRemoved { get; set; }
        public int ExpectedSustained { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return Description;
        }
    }

    [TestFixture]
    public class AdvisoryChangedNotifierTest
    {
        private AdvisoryChangedNotifier _advisoryChangedNotifier;
        private IMtaStsEntityConfig _mtaStsEntityConfig;
        private IMessageDispatcher _dispatcher;
        private ILogger<AdvisoryChangedNotifier> _log;
        private IEqualityComparer<AdvisoryMessage> _messageEqualityComparer;

        private static Guid infoId2 = new Guid("123a26ee-e01c-4673-99cb-badbe8545ba6");
        private static Guid warningId1 = new Guid("122ebd52-3d93-46e9-95de-659bf89ca2b8");
        private static Guid warningId2 = new Guid("a7541767-ed48-4862-bab7-dd9344caa9aa");
        private static Guid errorId1 = new Guid("b0e6747f-a6bd-42f8-b06d-e16fe24e76b4");
        private static Guid errorId2 = new Guid("8b52ca95-ba29-4f64-bd0b-873d877ad3b5");
        private static Guid infoId1 = new Guid("33c14e10-6223-46c0-abe0-82ef34a815be");

        [SetUp]
        public void Setup()
        {
            _mtaStsEntityConfig = A.Fake<IMtaStsEntityConfig>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _log = A.Fake<ILogger<AdvisoryChangedNotifier>>();
            _messageEqualityComparer = new MessageEqualityComparer();

            _advisoryChangedNotifier =
                new AdvisoryChangedNotifier(_dispatcher, _mtaStsEntityConfig, _messageEqualityComparer, _log);
        }

        [TestCaseSource(nameof(ExerciseEqualityComparersTestPermutations))]
        public Task ExerciseEqualityComparers(AdvisoryChangedNotifierTestCase testCase)
        {
            _advisoryChangedNotifier.Handle("test.gov.uk", testCase.CurrentMessages, testCase.NewMessages);

            if (testCase.ExpectedAdded > 0)
            {
                A.CallTo(() => _dispatcher.Dispatch(A<MtaStsAdvisoryAdded>.That.Matches(x => x.Id == "test.gov.uk" && x.Messages.Count == testCase.ExpectedAdded), A<string>._))
                 .MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => _dispatcher.Dispatch(A<MtaStsAdvisoryAdded>.That.Matches(x => x.Id == "test.gov.uk" && x.Messages.Count == testCase.ExpectedAdded), A<string>._))
                 .MustNotHaveHappened();
            }

            if (testCase.ExpectedSustained > 0)
            {
                A.CallTo(() => _dispatcher.Dispatch(A<MtaStsAdvisorySustained>.That.Matches(x => x.Id == "test.gov.uk" && x.Messages.Count == testCase.ExpectedSustained), A<string>._))
                 .MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => _dispatcher.Dispatch(A<MtaStsAdvisorySustained>.That.Matches(x => x.Id == "test.gov.uk" && x.Messages.Count == testCase.ExpectedSustained), A<string>._))
                 .MustNotHaveHappened();
            }

            if (testCase.ExpectedRemoved > 0)
            {
                A.CallTo(() => _dispatcher.Dispatch(A<MtaStsAdvisoryRemoved>.That.Matches(x => x.Id == "test.gov.uk" && x.Messages.Count == testCase.ExpectedRemoved), A<string>._))
                 .MustHaveHappenedOnceExactly();
            }
            else
            {
                A.CallTo(() => _dispatcher.Dispatch(A<MtaStsAdvisoryRemoved>.That.Matches(x => x.Id == "test.gov.uk" && x.Messages.Count == testCase.ExpectedRemoved), A<string>._))
                 .MustNotHaveHappened();
            }

            return Task.CompletedTask;
        }

        private static IEnumerable<AdvisoryChangedNotifierTestCase> ExerciseEqualityComparersTestPermutations()
        {
            AdvisoryMessage error1 = new AdvisoryMessage(errorId1, AdvisoryType.Error, "test", "test");
            AdvisoryMessage error2 = new AdvisoryMessage(errorId2, AdvisoryType.Error, "test", "test");
            AdvisoryMessage warning1 = new AdvisoryMessage(warningId1, AdvisoryType.Warning, "test", "test");
            AdvisoryMessage warning2 = new AdvisoryMessage(warningId2, AdvisoryType.Warning, "test", "test");
            AdvisoryMessage info1 = new AdvisoryMessage(infoId1, AdvisoryType.Info, "test", "test");
            AdvisoryMessage info2 = new AdvisoryMessage(infoId2, AdvisoryType.Info, "test", "test");

            AdvisoryChangedNotifierTestCase test1 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = new List<AdvisoryMessage>(),
                NewMessages = new List<AdvisoryMessage> { error1 },
                ExpectedAdded = 1,
                ExpectedRemoved = 0,
                ExpectedSustained = 0,
                Description = "0 -> 1 advisory should produce 1 advisory added"
            };

            AdvisoryChangedNotifierTestCase test2 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = new List<AdvisoryMessage> { error1 },
                NewMessages = new List<AdvisoryMessage>(),
                ExpectedAdded = 0,
                ExpectedRemoved = 1,
                ExpectedSustained = 0,
                Description = "1 -> 0 advisory should produce 1 advisory removed"
            };

            AdvisoryChangedNotifierTestCase test3 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = new List<AdvisoryMessage> { error1 },
                NewMessages = new List<AdvisoryMessage> { error1 },
                ExpectedAdded = 0,
                ExpectedRemoved = 0,
                ExpectedSustained = 1,
                Description = "1 -> 1 advisory should produce 1 advisory sustained"
            };

            AdvisoryChangedNotifierTestCase test4 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = new List<AdvisoryMessage> { error1 },
                NewMessages = new List<AdvisoryMessage> { error1, error2 },
                ExpectedAdded = 1,
                ExpectedRemoved = 0,
                ExpectedSustained = 1,
                Description = "1 -> 2 advisory should produce 1 advisory added, 1 advisory sustained"
            };

            AdvisoryChangedNotifierTestCase test5 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = new List<AdvisoryMessage> { error1, error2, warning1 },
                NewMessages = new List<AdvisoryMessage> { warning2, info1, info2 },
                ExpectedAdded = 3,
                ExpectedRemoved = 3,
                ExpectedSustained = 0,
                Description = "3 different -> 3 different advisory should produce 3 advisory added, 3 advisory removed"
            };

            AdvisoryChangedNotifierTestCase test6 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = new List<AdvisoryMessage> { error1, error2, warning1, warning2, info1 },
                NewMessages = new List<AdvisoryMessage> { warning2, info1, info2 },
                ExpectedAdded = 1,
                ExpectedRemoved = 3,
                ExpectedSustained = 2,
                Description = "should produce 1 advisory added, 3 advisories removed, 2 advisories sustained"
            };

            AdvisoryChangedNotifierTestCase test7 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = null,
                NewMessages = null,
                ExpectedAdded = 0,
                ExpectedRemoved = 0,
                ExpectedSustained = 0,
                Description = "when both current and new messages are null, should produce 0 advisories"
            };

            AdvisoryChangedNotifierTestCase test8 = new AdvisoryChangedNotifierTestCase
            {
                CurrentMessages = null,
                NewMessages = new List<AdvisoryMessage> { error1 },
                ExpectedAdded = 1,
                ExpectedRemoved = 0,
                ExpectedSustained = 0,
                Description = "when current messages is null and there is one new message, should produce 1 added advisory"
            };

            yield return test1;
            yield return test2;
            yield return test3;
            yield return test4;
            yield return test5;
            yield return test6;
            yield return test7;
            yield return test8;
        }
    }
}