using System;
using System.Collections.Generic;
using FakeItEasy;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.Contracts.PolicyFetcher;
using MailCheck.MtaSts.PolicyFetcher.Domain.Errors;
using MailCheck.MtaSts.PolicyFetcher.Parsing;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.MtaSts.PolicyFetcher.Test.Parsing
{
    [TestFixture]
    public class MtaStsPolicyParserTests
    {
        private MtaStsPolicyParser _parser;
        private List<IKeyParser> _parsers;
        private ILogger<PolicyFetcher> _log;
        private IKeyParser _keyParser;

        [SetUp]
        public void SetUp()
        {
            _keyParser =A.Fake<IKeyParser>();
            _parsers = new List<IKeyParser>{_keyParser};
            _log = A.Fake<ILogger<PolicyFetcher>>();
            _parser = new MtaStsPolicyParser(_parsers, _log);
        }

        [Test]
        public void ShouldReturnNoPolicyErrorWhenNoPolicyFound()
        {
            string domain = "ncsc.gov.uk";
            string responseBody = "";
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();
            _parser.Parse(domain, responseBody, errors);
            
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(MessageType.warning, errors[0].MessageType);
            Assert.AreEqual("No MTA-STS policy configured.", errors[0].Text);
        }

        [Test]
        public void ShouldNotReturnNoPolicyErrorWhenErrorFetchingPolicy()
        {
            string domain = "ncsc.gov.uk";
            string responseBody = "";
            string errorText = "Timed Out when fetching policy file.";
            List<AdvisoryMessage> errors = new List<AdvisoryMessage> { new FailedToFetch(errorText, string.Empty) };
            _parser.Parse(domain, responseBody, errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(MessageType.error, errors[0].MessageType);
            Assert.AreEqual(errorText, errors[0].Text);
        }

        [Test]
        public void ShouldEvaluateKnownKey()
        {
            string responseBody = "key: rawValue";
            IKeyParser keyParser = A.Fake<IKeyParser>();
            A.CallTo(() => keyParser.KeyType).Returns("key");
            A.CallTo(() => keyParser.MaxOccurrences).Returns(1);
 
            A.CallTo(() => keyParser.Parse(A<List<Key>>._, responseBody, "key", "rawValue")).Returns(new EvaluationResult<Key>(new MxKey("evaluatedValue", "rawValue")));
            _parsers.Add(keyParser);

            _parser = new MtaStsPolicyParser(new List<IKeyParser>{ keyParser }, _log);

            MtaStsPolicyResult result = _parser.Parse(string.Empty, responseBody, null);

            Assert.AreEqual(0, result.Errors.Count);

            Assert.AreEqual(1, result.Keys.Count);
            Assert.AreEqual("evaluatedValue", result.Keys[0].Value);
            Assert.AreEqual(null, result.Keys[0].Explanation);
            Assert.AreEqual("rawValue", result.Keys[0].RawValue);
            Assert.AreEqual("MxKey", result.Keys[0].Type);
        }

        [Test]
        public void ShouldReturnErrorForBadlyFormattedKey()
        {
            string responseBody = " key: rawValue";
            IKeyParser keyParser = A.Fake<IKeyParser>();
            A.CallTo(() => keyParser.KeyType).Returns("key");
            A.CallTo(() => keyParser.MaxOccurrences).Returns(1);

            _parser = new MtaStsPolicyParser(new List<IKeyParser> { keyParser }, _log);

            MtaStsPolicyResult result = _parser.Parse(string.Empty, responseBody, null);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("mailcheck.mtasts.invalidKey", result.Errors[0].Name);

            Assert.AreEqual("rawValue", result.Keys[0].Value);
            Assert.AreEqual("Invalid key", result.Keys[0].Explanation);
            Assert.AreEqual(" key: rawValue", result.Keys[0].RawValue);
            Assert.AreEqual("UnknownKey", result.Keys[0].Type);
        }

        [Test]
        public void ShouldReturnErrorForMissingValue()
        {
            string responseBody = "key";
            IKeyParser keyParser = A.Fake<IKeyParser>();
            A.CallTo(() => keyParser.KeyType).Returns("key");
            A.CallTo(() => keyParser.MaxOccurrences).Returns(1);

            _parser = new MtaStsPolicyParser(new List<IKeyParser> { keyParser }, _log);

            MtaStsPolicyResult result = _parser.Parse(string.Empty, responseBody, null);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("mailcheck.mtasts.keyValueNotFound", result.Errors[0].Name);

            Assert.AreEqual(0, result.Keys.Count);
        }

        [Test]
        public void ShouldReturnErrorWhenFailedToParse()
        {
            string responseBody = "key: value";
            IKeyParser keyParser = A.Fake<IKeyParser>();
            A.CallTo(() => keyParser.Parse(A<List<Key>>._, A<string>._, A<string>._, A<string>._)).Throws<Exception>();
            A.CallTo(() => keyParser.KeyType).Returns("key");
            A.CallTo(() => keyParser.MaxOccurrences).Returns(1);

            _parser = new MtaStsPolicyParser(new List<IKeyParser> { keyParser }, _log);

            MtaStsPolicyResult result = _parser.Parse(string.Empty, responseBody, null);

            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("mailcheck.mtasts.failedToParse", result.Errors[0].Name);

            Assert.AreEqual(0, result.Keys.Count);
        }
    }
}
