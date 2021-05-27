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
    public class MtaStsShouldBeEnforcedTests
    {
        private MtaStsShouldBeEnforced _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new MtaStsShouldBeEnforced();
        }

        [Test]
        public async Task NoErrorWhenPolicyEnforced()
        {
            MtaStsPolicyResult result = new MtaStsPolicyResult("version: STSv1 mode: enforce",
                new List<Key>
                {
                    new VersionKey("STSv1", "version: STSv1"),
                    new ModeKey("enforce", "mode: enforce")
                }, new List<AdvisoryMessage>());

            var results = await _rule.Evaluate(result);
            Assert.That(results.Any(), Is.False);
        }

        [Test]
        public async Task ErrorWhenPolicyEnforced()
        {
            MtaStsPolicyResult result = new MtaStsPolicyResult("version: STSv1 mode: testing",
                new List<Key>
                {
                    new VersionKey("STSv1", "version: STSv1"),
                    new ModeKey("testing", "mode: testing")
                }, new List<AdvisoryMessage>());

            var results = await _rule.Evaluate(result);
            Assert.That(results.Any(), Is.True);
        }
    }
}