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
    public class ModeParserTests
    {
        private ModeParser _modeParser;

        [SetUp]
        public void SetUp()
        {
            _modeParser = new ModeParser();
        }

        [Test]
        public void ShouldSuccessfullyParseEnforceMode()
        {
            string line = "mode: enforce";
            List<Key> keys = new List<Key>();

            EvaluationResult<Key> result = _modeParser.Parse(keys, line, "mode", "enforce");

            Assert.AreEqual(0, result.AdvisoryMessages.Count);
            Assert.AreEqual("ModeKey", result.Item.Type);
            Assert.AreEqual("Enforce mode: Sending MTAs MUST NOT deliver the " +
                            "message to hosts that fail MX matching or certificate validation " +
                            "or that do not support STARTTLS.", result.Item.Explanation);
        }

        [Test]
        public void ShouldSuccessfullyParseTestingMode()
        {
            string line = "mode: testing";
            List<Key> keys = new List<Key>();

            EvaluationResult<Key> result = _modeParser.Parse(keys, line, "mode", "testing");

            Assert.AreEqual(0, result.AdvisoryMessages.Count);
            Assert.AreEqual("ModeKey", result.Item.Type);
            Assert.AreEqual("Testing mode: Sending MTAs that also implement the TLSRPT (TLS Reporting) specification " +
                            "[RFC8460](https://tools.ietf.org/html/rfc8460) send a report " +
                            "indicating policy application failures (as long as TLSRPT is also " +
                            "implemented by the recipient domain); in any case, messages may be " +
                            "delivered as though there were no MTA-STS validation failure.", result.Item.Explanation);
        }

        [Test]
        public void ShouldSuccessfullyParseNoneMode()
        {
            string line = "mode: none";
            List<Key> keys = new List<Key>();

            EvaluationResult<Key> result = _modeParser.Parse(keys, line, "mode", "none");

            Assert.AreEqual(0, result.AdvisoryMessages.Count);
            Assert.AreEqual("ModeKey", result.Item.Type);
            Assert.AreEqual("None mode: Sending MTAs should treat the Policy Domain as though it does not have any active policy.", result.Item.Explanation);
        }

        [Test]
        public void ShouldParseWithErrorOnInvalidMode()
        {
            string line = "mode: invalid";
            List<Key> keys = new List<Key>();

            EvaluationResult<Key> result = _modeParser.Parse(keys, line, "mode", "invalid");

            Assert.AreEqual(1, result.AdvisoryMessages.Count);
            Assert.AreEqual("Invalid mode key.", result.AdvisoryMessages[0].Text);
            Assert.AreEqual("The mode key must be one of \"enforce\", \"testing\", or \"none\".", result.AdvisoryMessages[0].MarkDown);
            Assert.AreEqual("ModeKey", result.Item.Type);
            Assert.AreEqual("Invalid mode key, mode should be set to one of enforce, testing or none.", result.Item.Explanation);
        }
    }
}
