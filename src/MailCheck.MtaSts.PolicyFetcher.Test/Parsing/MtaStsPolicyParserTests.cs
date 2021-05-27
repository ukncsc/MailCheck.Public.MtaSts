using System.Collections.Generic;
using Amazon.XRay.Model;
using FakeItEasy;
using MailCheck.MtaSts.Contracts;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Keys;
using MailCheck.MtaSts.Contracts.Tags;
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

        [SetUp]
        public void SetUp()
        {
            _parsers = A.Fake<List<IKeyParser>>();
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
            Assert.AreEqual(AdvisoryType.Warning, errors[0].AdvisoryType);
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
            Assert.AreEqual(AdvisoryType.Error, errors[0].AdvisoryType);
            Assert.AreEqual(errorText, errors[0].Text);
        }
    }
}
