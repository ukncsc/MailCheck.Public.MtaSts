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
    public class MaxAgeParserTests
    {
        private MaxAgeParser _maxAgeParser;

        [SetUp]
        public void SetUp()
        {
            _maxAgeParser = new MaxAgeParser();
        }

        [Test]
        public void ShouldSuccessfullyParseValidMaxAge()
        {
            string line = "max_age: 86400";
            List<Key> keys = new List<Key>();

            EvaluationResult<Key> result = _maxAgeParser.Parse(keys, line, "max_age", "86400");

            Assert.AreEqual(0, result.AdvisoryMessages.Count);
            Assert.AreEqual("MaxAgeKey", result.Item.Type);
            Assert.AreEqual("Max lifetime of the policy is 86400 seconds.", result.Item.Explanation);
        }

        [Test]
        public void ShouldParseWithErrorOnInvalidMaxAge()
        {
            string line = "max_age: wrong";
            List<Key> keys = new List<Key>();

            EvaluationResult<Key> result = _maxAgeParser.Parse(keys, line, "max_age", "wrong");

            Assert.AreEqual(1, result.AdvisoryMessages.Count);
            Assert.AreEqual("Invalid max_age key.", result.AdvisoryMessages[0].Text);
            Assert.AreEqual("The max_age key must be a plaintext non-negative integer with a maximum value of 31557600.", result.AdvisoryMessages[0].MarkDown);
            Assert.AreEqual("MaxAgeKey", result.Item.Type);
            Assert.AreEqual("'wrong' is an invalid value for max_age, please use an integer between 0 and 31557600 seconds.", result.Item.Explanation);
        }
    }
}
