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
    public class MxParserTests
    {
        private MxParser _mxParser;

        [SetUp]
        public void SetUp()
        {
            _mxParser = new MxParser();
        }

        [Test]
        public void ShouldSuccessfullyParseValidMx()
        {
            string line = "mx: ncsc-gov-uk.mail.protection.outlook.com";
            List<Key> keys = new List<Key>();

            EvaluationResult<Key> result = _mxParser.Parse(keys, line, "mx", "ncsc-gov-uk.mail.protection.outlook.com");

            Assert.AreEqual(0, result.AdvisoryMessages.Count);
            Assert.AreEqual("MxKey", result.Item.Type);
            Assert.AreEqual("Mail for this domain may be handled by MX ncsc-gov-uk.mail.protection.outlook.com.", result.Item.Explanation);
        }

        [Test]
        public void ShouldSuccessfullyParseValidMxWithWildcard()
        {
            string line = "mx: *.mail.protection.outlook.com";
            List<Key> keys = new List<Key>();

            EvaluationResult<Key> result = _mxParser.Parse(keys, line, "mx", "*.mail.protection.outlook.com");

            Assert.AreEqual(0, result.AdvisoryMessages.Count);
            Assert.AreEqual("MxKey", result.Item.Type);
            Assert.AreEqual("Mail for this domain may be handled by MX *.mail.protection.outlook.com.", result.Item.Explanation);
        }

        [TestCase("mx", "@@@@")]
        [TestCase("mx", "ncsc-gov-uk.mail.protection.outlook.com.")]
        [TestCase("mx", "ncsc-gov-uk.mail.protection.outlook.com ")]
        [TestCase("mx", " ncsc-gov-uk.mail.protection.outlook.com")]
        [TestCase("mx", "*.protection.outlook.com ")]
        [TestCase("mx", " *.protection.outlook.com")]
        public void ShouldParseWithErrorOnInvalidMx(string key, string value)
        {
            List<Key> keys = new List<Key>();

            EvaluationResult<Key> result = _mxParser.Parse(keys, null, key, value);

            Assert.AreEqual(1, result.AdvisoryMessages.Count);
            Assert.AreEqual("Invalid mx key.", result.AdvisoryMessages[0].Text);
            Assert.AreEqual("Valid patterns can be either fully specified names (\"example.com\") or suffixes prefixed " +
                "by a wildcard (\"*.example.net\"). If a policy specifies more than one MX, each MX MUST have its " +
                "own \"mx:\" key, and each MX key/value pair MUST be on its own line in the policy file.", result.AdvisoryMessages[0].MarkDown);
            Assert.AreEqual("MxKey", result.Item.Type);
            Assert.AreEqual($"{value} is in an invalid host format.", result.Item.Explanation);
        }
    }
}
