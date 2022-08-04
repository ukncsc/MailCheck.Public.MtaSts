using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Contracts.PolicyFetcher;
using MailCheck.MtaSts.PolicyFetcher.Parsing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.MtaSts.PolicyFetcher.Test
{
    [TestFixture]
    public class PolicyFetcherTests
    {
        private PolicyFetcher _policyFetcher;
        private IMtaStsPolicyParser _parser;
        private TestLogger _log;
        private IEvaluator<MtaStsPolicyResult> _evaluator;
        private HttpMessageHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _parser = A.Fake<IMtaStsPolicyParser>();
            _log = new TestLogger();
            _evaluator = A.Fake<IEvaluator<MtaStsPolicyResult>>();
            _handler = A.Fake<HttpMessageHandler>();

            _policyFetcher = new PolicyFetcher(_parser, _log, _evaluator) { Handler = _handler };
        }

        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.Forbidden)]
        public async Task ProcessAddsErrorOnNotFoundOrForbidden(HttpStatusCode statusCode)
        {
            HttpResponseMessage notFoundResponse = new HttpResponseMessage(statusCode);
            A.CallTo(_handler).Where(x => x.Method.Name == "SendAsync").WithReturnType<Task<HttpResponseMessage>>().Returns(notFoundResponse);

            MtaStsPolicyResult resultFromParser = A.Fake<MtaStsPolicyResult>();
            A.CallTo(() => _parser.Parse(A<string>._, A<string>._,
                A<List<AdvisoryMessage>>.That.Matches(
                    x => x.Any(y => ((MtaStsAdvisoryMessage)y).Name == "mailcheck.mtasts.failedToFetch"))))
                .Returns(resultFromParser);

            MtaStsPolicyResult result = await _policyFetcher.Process("testDomain");

            Assert.AreSame(resultFromParser, result);
            Assert.That(_log.Messages.Count(x => x.level == LogLevel.Warning && x.message == "Could not fetch policy file for testDomain") == 1);
        }
         
        [TestCase(HttpStatusCode.Ambiguous)]
        [TestCase(HttpStatusCode.BadGateway)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Conflict)]
        [TestCase(HttpStatusCode.Continue)]
        [TestCase(HttpStatusCode.EarlyHints)]
        [TestCase(HttpStatusCode.ExpectationFailed)]
        [TestCase(HttpStatusCode.FailedDependency)]
        [TestCase(HttpStatusCode.GatewayTimeout)]
        [TestCase(HttpStatusCode.Gone)]
        [TestCase(HttpStatusCode.HttpVersionNotSupported)]
        [TestCase(HttpStatusCode.InsufficientStorage)]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.LengthRequired)]
        [TestCase(HttpStatusCode.Locked)]
        [TestCase(HttpStatusCode.LoopDetected)]
        [TestCase(HttpStatusCode.MethodNotAllowed)]
        [TestCase(HttpStatusCode.MisdirectedRequest)]
        [TestCase(HttpStatusCode.Moved)]
        [TestCase(HttpStatusCode.MovedPermanently)]
        [TestCase(HttpStatusCode.MultipleChoices)]
        [TestCase(HttpStatusCode.NotAcceptable)]
        [TestCase(HttpStatusCode.NotExtended)]
        [TestCase(HttpStatusCode.NotImplemented)]
        [TestCase(HttpStatusCode.NotModified)]
        [TestCase(HttpStatusCode.PaymentRequired)]
        [TestCase(HttpStatusCode.PermanentRedirect)]
        [TestCase(HttpStatusCode.PreconditionFailed)]
        [TestCase(HttpStatusCode.PreconditionRequired)]
        [TestCase(HttpStatusCode.Processing)]
        [TestCase(HttpStatusCode.ProxyAuthenticationRequired)]
        [TestCase(HttpStatusCode.Redirect)]
        [TestCase(HttpStatusCode.RedirectKeepVerb)]
        [TestCase(HttpStatusCode.RedirectMethod)]
        [TestCase(HttpStatusCode.RequestedRangeNotSatisfiable)]
        [TestCase(HttpStatusCode.RequestEntityTooLarge)]
        [TestCase(HttpStatusCode.RequestHeaderFieldsTooLarge)]
        [TestCase(HttpStatusCode.RequestTimeout)]
        [TestCase(HttpStatusCode.RequestUriTooLong)]
        [TestCase(HttpStatusCode.SeeOther)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        [TestCase(HttpStatusCode.SwitchingProtocols)]
        [TestCase(HttpStatusCode.TemporaryRedirect)]
        [TestCase(HttpStatusCode.TooManyRequests)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.UnavailableForLegalReasons)]
        [TestCase(HttpStatusCode.UnprocessableEntity)]
        [TestCase(HttpStatusCode.UnsupportedMediaType)]
        [TestCase(HttpStatusCode.Unused)]
        [TestCase(HttpStatusCode.UpgradeRequired)]
        [TestCase(HttpStatusCode.UseProxy)]
        [TestCase(HttpStatusCode.VariantAlsoNegotiates)]
        public async Task ProcessAddsErrorOnOtherErrorStatus(HttpStatusCode statusCode)
        {
            HttpResponseMessage notFoundResponse = new HttpResponseMessage(statusCode);
            A.CallTo(_handler).Where(x => x.Method.Name == "SendAsync").WithReturnType<Task<HttpResponseMessage>>().Returns(notFoundResponse);

            MtaStsPolicyResult resultFromParser = A.Fake<MtaStsPolicyResult>();
            A.CallTo(() => _parser.Parse(A<string>._, A<string>._,
                    A<List<AdvisoryMessage>>.That.Matches(
                        x => x.Any(y => ((MtaStsAdvisoryMessage)y).Name == "mailcheck.mtasts.failedToFetch"))))
                .Returns(resultFromParser);

            MtaStsPolicyResult result = await _policyFetcher.Process("testDomain");

            Assert.AreSame(resultFromParser, result);
            Assert.That(_log.Messages.Count(x => x.level == LogLevel.Error && x.message == "Could not fetch policy file for testDomain") == 1);
        }

        [Test]
        public async Task ProcessAddsErrorOnSslError()
        {
            A.CallTo(_handler).Where(x => x.Method.Name == "SendAsync").WithReturnType<Task<HttpResponseMessage>>()
                .Throws(new HttpRequestException(string.Empty, new AuthenticationException()));

            MtaStsPolicyResult resultFromParser = A.Fake<MtaStsPolicyResult>();
            A.CallTo(() => _parser.Parse(A<string>._, A<string>._,
                    A<List<AdvisoryMessage>>.That.Matches(
                        x => x.Any(y => ((MtaStsAdvisoryMessage)y).Name == "mailcheck.mtasts.failedToFetch"))))
                .Returns(resultFromParser);

            MtaStsPolicyResult result = await _policyFetcher.Process("testDomain");

            Assert.AreSame(resultFromParser, result);
            Assert.That(_log.Messages.Count(x => x.level == LogLevel.Warning && x.message == "Could not fetch policy file for testDomain") == 1);
        }

        [Test]
        public async Task ProcessAddsErrorOnOtherHttpRequestExceptionError()
        {
            A.CallTo(_handler).Where(x => x.Method.Name == "SendAsync").WithReturnType<Task<HttpResponseMessage>>()
                .Throws(new HttpRequestException());

            MtaStsPolicyResult resultFromParser = A.Fake<MtaStsPolicyResult>();
            A.CallTo(() => _parser.Parse(A<string>._, A<string>._,
                    A<List<AdvisoryMessage>>.That.Matches(
                        x => x.Any(y => ((MtaStsAdvisoryMessage)y).Name == "mailcheck.mtasts.failedToFetch"))))
                .Returns(resultFromParser);

            MtaStsPolicyResult result = await _policyFetcher.Process("testDomain");

            Assert.AreSame(resultFromParser, result);
            Assert.That(_log.Messages.Count(x => x.level == LogLevel.Error && x.message == "Could not fetch policy file for testDomain") == 1);
        }

        [Test]
        public async Task ProcessAddsErrorOnOperationCancelledError()
        {
            A.CallTo(_handler).Where(x => x.Method.Name == "SendAsync").WithReturnType<Task<HttpResponseMessage>>()
                .Throws(new OperationCanceledException());

            MtaStsPolicyResult resultFromParser = A.Fake<MtaStsPolicyResult>();
            A.CallTo(() => _parser.Parse(A<string>._, A<string>._,
                    A<List<AdvisoryMessage>>.That.Matches(
                        x => x.Any(y => ((MtaStsAdvisoryMessage)y).Name == "mailcheck.mtasts.failedToFetch"))))
                .Returns(resultFromParser);

            MtaStsPolicyResult result = await _policyFetcher.Process("testDomain");

            Assert.AreSame(resultFromParser, result);
            Assert.That(_log.Messages.Count(x => x.level == LogLevel.Error && x.message == "Could not fetch policy file for testDomain") == 1);
        }

        [Test]
        public void ProcessThrowsOnOtherError()
        {
            A.CallTo(_handler).Where(x => x.Method.Name == "SendAsync").WithReturnType<Task<HttpResponseMessage>>()
                .Throws(new Exception());

            Assert.ThrowsAsync<Exception>(() => _policyFetcher.Process("testDomain"));
        }

        [Test]
        public async Task ProcessEncodesResponseAsAscii()
        {
            byte[] utfBytes = new byte[] { 239, 187, 191, 118, 101, 114, 115, 105, 111, 110, 58, 32, 83, 84,
                83, 118, 49, 13, 10, 109, 111, 100, 101, 58, 32, 101, 110, 102, 111, 114, 99, 101, 13, 10,
                109, 97, 120, 95, 97, 103, 101, 58, 32, 49, 48, 51, 54, 56, 48, 48, 48, 13, 10, 109, 120,
                58, 32, 104, 105, 103, 104, 119, 97, 121, 115, 101, 110, 103, 108, 97, 110, 100, 45, 99,
                111, 45, 117, 107, 46, 109, 97, 105, 108, 46, 112, 114, 111, 116, 101, 99, 116, 105, 111,
                110, 46, 111, 117, 116, 108, 111, 111, 107, 46, 99, 111, 109 };

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(utfBytes)
            };

            A.CallTo(_handler).Where(x => x.Method.Name == "SendAsync").WithReturnType<Task<HttpResponseMessage>>().Returns(response);

            A.CallTo(() => _parser.Parse(A<string>._, A<string>._, A<List<AdvisoryMessage>>._))
                 .ReturnsLazily((string domain, string responseData, List<AdvisoryMessage> error) => new MtaStsPolicyResult(responseData, new List<Contracts.Keys.Key>(), new List<MtaStsAdvisoryMessage>()));

            MtaStsPolicyResult result = await _policyFetcher.Process("testDomain");

            Assert.That(result.RawValue.StartsWith("???version:"));
        }
    }

    public class TestLogger : ILogger<PolicyFetcher>
    {
        public List<(LogLevel level, string message)> Messages = new List<(LogLevel level, string message)>();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            IReadOnlyList<KeyValuePair<string, object>> kvps = (IReadOnlyList<KeyValuePair<string, object>>)state;

            Messages.Add((logLevel, kvps[0].Value.ToString()));
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}
