using System.Collections.Generic;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Contracts.Tags;
using MailCheck.MtaSts.Evaluator.Explainers;
using NUnit.Framework;

namespace MailCheck.MtaSts.Evaluator.Test.Explainers
{
    [TestFixture]
    public class VersionTagExplainerTests
    {
        private VersionTagExplainer _versionTagExplainer;

        [SetUp]
        public void SetUp()
        {
            _versionTagExplainer = new VersionTagExplainer();
        }

        [Test]
        public void ShouldAddExplanationIfVersionTagPresent()
        {
            MtaStsRecord mtaStsRecord = new MtaStsRecord(null, new List<string>(), new List<Tag> { new VersionTag("testVersion", "testVersion") });
            _versionTagExplainer.AddExplanation(mtaStsRecord);

            string explanation = mtaStsRecord.Tags[0].Explanation;
            Assert.AreEqual("Version. Currently, only \"STSv1\" is supported.", explanation);
        }

        [Test]
        public void ShouldNotAddExplanationIfVersionTagNotPresent()
        {
            MtaStsRecord mtaStsRecord = new MtaStsRecord(null, new List<string>(), new List<Tag> { new PolicyVersionIdTag("testPolicyVersion", "testPolicyVersion") });
            _versionTagExplainer.AddExplanation(mtaStsRecord);

            string explanation = mtaStsRecord.Tags[0].Explanation;
            Assert.IsNull(explanation);
        }
    }
}
