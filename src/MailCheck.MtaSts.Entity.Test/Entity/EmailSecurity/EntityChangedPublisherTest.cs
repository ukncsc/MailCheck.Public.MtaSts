using System;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Contracts.Entity;
using MailCheck.MtaSts.Entity.Config;
using Microsoft.Extensions.Logging;
using MailCheck.Common.Contracts.Messaging;
using NUnit.Framework;
using MailCheck.MtaSts.Entity.Entity;
using System.Collections.Generic;
using MailCheck.MtaSts.Entity.Entity.EmailSecurity;
using MailCheck.MtaSts.Contracts.PolicyFetcher;
using MailCheck.MtaSts.Contracts.Keys;

namespace MailCheck.MtaSts.Entity.Test.Entity
{
    [TestFixture]
    public class EntityChangedPublisherTest
    {
        private IMtaStsEntityConfig _mtaStsEntityConfig;
        private IMessageDispatcher _dispatcher;
        private ILogger<EntityChangedPublisher> _log;
        private EntityChangedPublisher _entityChangedPublisher;

        [SetUp]
        public void SetUp()
        {
            _mtaStsEntityConfig = A.Fake<IMtaStsEntityConfig>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _log = A.Fake<ILogger<EntityChangedPublisher>>();

            _entityChangedPublisher = new EntityChangedPublisher(_mtaStsEntityConfig, _dispatcher, _log);
        }

        [Test]
        public void ShouldDispatchEntityChanged()
        {
            List<string> mxHosts = new List<string> { "mail.sabp.nhs.uk"};

            string stateRawValue = "version: STSv1\nmode: testing\nmx: sabp-nhs-uk.mail.protection.outlook.com\nmax_age: 86400\n\n";

            List<Key> stateKeys = new List<Key>
            {
                new VersionKey("STSv1", "version: STSv1"),
                new ModeKey("testing", "mode: testing"),
                new MxKey(mxHosts[0], $"mx: {mxHosts[0]}"),
                new MaxAgeKey("86400", "max_age: 86400")
            };

            MtaStsPolicyResult stateResult = new MtaStsPolicyResult(stateRawValue, stateKeys, new List<MtaStsAdvisoryMessage>());

            MtaStsEntityState state = new MtaStsEntityState("test.gov.uk", 2, MtaStsState.Evaluated, DateTime.UtcNow)
            {
                Policy = stateResult
            };

            A.CallTo(() => _mtaStsEntityConfig.RecordType).Returns("MTASTS");

            _entityChangedPublisher.Publish("test.gov.uk", state, nameof(MtaStsPolicyFetched));

            A.CallTo(() => _dispatcher.Dispatch(A<EntityChanged>.That.Matches(_ =>
                _.Id == "test.gov.uk" &&
                _.RecordType == "MTASTS" &&
                _.ReasonForChange == "MtaStsPolicyFetched" &&
                ((MtaStsEntityState)_.NewEntityDetail).Policy.RawValue == stateRawValue), A<string>._)
            ).MustHaveHappenedOnceExactly();
        }
    }
}
