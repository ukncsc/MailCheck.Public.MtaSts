using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.Contracts.PolicyFetcher;
using MailCheck.MtaSts.PolicyFetcher.Rules;
using NUnit.Framework;

namespace MailCheck.MtaSts.PolicyFetcher.Test.Rules
{
    [TestFixture]
    public class MaxAgeStrongKeyRuleTests
    {
        private MaxAgeStrongKeyRule _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new MaxAgeStrongKeyRule();
        }

        [Test]
        public async Task EnforceModeWithStrongKey()
        {
            MtaStsPolicyResult result = new MtaStsPolicyResult("version: STSv1 mode: enforce",
                new List<Key>
                {
                    new VersionKey("STSv1", "version: STSv1"),
                    new ModeKey("enforce", "mode: enforce"),
                    new ModeKey("1209700", "max_age: 1209700")
                }, new List<AdvisoryMessage>());

            var results = await _rule.Evaluate(result);
            Assert.That(results.Any(), Is.False);
        }

        [Test]
        public async Task EnforceModeWithWeakKey()
        {
            MtaStsPolicyResult result = new MtaStsPolicyResult("version: STSv1 mode: enforce",
                new List<Key>
                {
                    new VersionKey("STSv1", "version: STSv1"),
                    new ModeKey("enforce", "mode: enforce"),
                    new MaxAgeKey("1209500", "max_age: 1209500")
                }, new List<AdvisoryMessage>());

            var results = await _rule.Evaluate(result);
            Assert.That(results.Any(), Is.True);
            Assert.That(results[0].AdvisoryType, Is.EqualTo(AdvisoryType.Warning));
            Assert.That(results[0].MessageDisplay, Is.EqualTo(MessageDisplay.Standard));
            Assert.That(results[0].MarkDown,
                Is.EqualTo(
                    "MTA-STS protects against DNS hijacking or denial of service attacks by caching the policy for a long period of time - longer than an attack can be sustained or continue unnoticed. By setting your max age less than two weeks you will not have the full protection against these attacks. Clients should pick up changes daily by checking the version id in the DNS record, so there is no need to configure a lower max age if you are expecting to make changes."));
            Assert.That(results[0].Text, Is.EqualTo("Policy max age is too short"));
        }

        [Test]
        public async Task TestingModeWithStrongKey()
        {
            MtaStsPolicyResult result = new MtaStsPolicyResult("version: STSv1 mode: testing",
                new List<Key>
                {
                    new VersionKey("STSv1", "version: STSv1"),
                    new ModeKey("testing", "mode: testing"),
                    new ModeKey("86500", "max_age: 86500")
                }, new List<AdvisoryMessage>());

            var results = await _rule.Evaluate(result);
            Assert.That(results.Any(), Is.False);
        }

        [Test]
        public async Task TestingModeWithWeakKey()
        {
            MtaStsPolicyResult result = new MtaStsPolicyResult("version: STSv1 mode: testing",
                new List<Key>
                {
                    new VersionKey("STSv1", "version: STSv1"),
                    new ModeKey("testing", "mode: testing"),
                    new MaxAgeKey("86300", "max_age: 86300")
                }, new List<AdvisoryMessage>());

            var results = await _rule.Evaluate(result);
            Assert.That(results.Any(), Is.True);
            Assert.That(results[0].AdvisoryType, Is.EqualTo(AdvisoryType.Warning));
            Assert.That(results[0].MessageDisplay, Is.EqualTo(MessageDisplay.Standard));
            Assert.That(results[0].MarkDown,
                Is.EqualTo(
                    "MTA-STS protects against DNS hijacking or denial of service attacks by caching the policy for a long period of time - longer than an attack can be sustained or continue unnoticed. Google and other providers will ignore a policy completely if it has a max age of less than 24 hours as the RFC suggests the DNS record is checked daily."));
            Assert.That(results[0].Text, Is.EqualTo("Policy max age is too short"));
        }
    }
}