using System.Collections.Generic;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.Contracts.Tags;
using MailCheck.MtaSts.PolicyFetcher.Parsing;
using NUnit.Framework;

namespace MailCheck.MtaSts.PolicyFetcher.Test.Parsing
{
    [TestFixture]
    public class VersionParserTests
    {
        private VersionParser _versionParser;

        [SetUp]
        public void SetUp()
        {
            _versionParser = new VersionParser();
        }

        [Test]
        public void ShouldSuccessfullyParseValidVersion()
        {
            string line = "version: STSv1";
            List<Key> keys = new List<Key>();

            EvaluationResult<Key> result = _versionParser.Parse(keys, line, "version", "STSv1");

            Assert.AreEqual(0, result.AdvisoryMessages.Count);
            Assert.AreEqual("VersionKey", result.Item.Type);
            Assert.AreEqual("MTA-STS version. Currently, only STSv1 is supported.", result.Item.Explanation);
        }

        [Test]
        public void ShouldParseWithErrorOnInvalidVersion()
        {
            string line = "version: wrong";
            List<Key> keys = new List<Key>();

            EvaluationResult<Key> result = _versionParser.Parse(keys, line, "version", "wrong");

            Assert.AreEqual(1, result.AdvisoryMessages.Count);
            Assert.AreEqual("Invalid version key.", result.AdvisoryMessages[0].Text);
            Assert.AreEqual("Only STSv1 is supported for the version key.", result.AdvisoryMessages[0].MarkDown);
            Assert.AreEqual("VersionKey", result.Item.Type);
            Assert.AreEqual("wrong is invalid. Currently, only STSv1 is supported for the version key.", result.Item.Explanation);
        }
    }
}
