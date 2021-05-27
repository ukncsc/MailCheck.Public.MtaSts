using System.Collections.Generic;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Contracts.Tags;
using MailCheck.MtaSts.Evaluator.Explainers;
using NUnit.Framework;

namespace MailCheck.MtaSts.Evaluator.Test.Explainers
{
    [TestFixture]
    public class PolicyVersionIdTagExplainerTests
    {
        private PolicyVersionIdTagExplainer _policyVersionIdTagExplainer;

        [SetUp]
        public void SetUp()
        {
            _policyVersionIdTagExplainer = new PolicyVersionIdTagExplainer();
        }

        [Test]
        public void ShouldAddExplanationIfPolicyVersionTagPresent()
        {
            MtaStsRecord mtaStsRecord = new MtaStsRecord(null, new List<string>(), new List<Tag> { new PolicyVersionIdTag("testPolicyVersion", "testPolicyVersion") });
            _policyVersionIdTagExplainer.AddExplanation(mtaStsRecord);

            string explanation = mtaStsRecord.Tags[0].Explanation;
            Assert.AreEqual("A short string used to track policy updates.", explanation);
        }

        [Test]
        public void ShouldNotAddExplanationIfPolicyVersionTagNotPresent()
        {
            MtaStsRecord mtaStsRecord = new MtaStsRecord(null, new List<string>(), new List<Tag> { new VersionTag("testVersion", "testVersion") });
            _policyVersionIdTagExplainer.AddExplanation(mtaStsRecord);

            string explanation = mtaStsRecord.Tags[0].Explanation;
            Assert.IsNull(explanation);
        }
    }
}
