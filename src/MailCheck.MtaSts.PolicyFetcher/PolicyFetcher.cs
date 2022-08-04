using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Contracts.PolicyFetcher;
using MailCheck.MtaSts.PolicyFetcher.Domain.Errors;
using MailCheck.MtaSts.PolicyFetcher.Parsing;
using Microsoft.Extensions.Logging;

namespace MailCheck.MtaSts.PolicyFetcher
{
    public interface IPolicyFetcher
    {
        Task<MtaStsPolicyResult> Process(string domain);
    }

    public class PolicyFetcher : IPolicyFetcher
    {
        private readonly ILogger<PolicyFetcher> _log;
        private readonly IMtaStsPolicyParser _parser;
        private readonly IEvaluator<MtaStsPolicyResult> _evaluator;
        internal HttpMessageHandler Handler;

        public PolicyFetcher(IMtaStsPolicyParser parser, ILogger<PolicyFetcher> log, IEvaluator<MtaStsPolicyResult> evaluator)
        {
            _parser = parser;
            _log = log;
            _evaluator = evaluator;
        }

        public async Task<MtaStsPolicyResult> Process(string domain)
        {
            _log.LogInformation($"Fetching policy file for {domain}");
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();
            string url = $"https://mta-sts.{domain}/.well-known/mta-sts.txt";
            string responseBody = null;
            HttpResponseMessage responseMessage = null;
            using (HttpClient client = Handler != null ? new HttpClient(Handler) : new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(15);
                try
                {
                    responseMessage = await client.GetAsync(url);
                    responseMessage.EnsureSuccessStatusCode();

                    byte[] rawBody = await responseMessage.Content.ReadAsByteArrayAsync();
                    responseBody = Encoding.ASCII.GetString(rawBody);

                    _log.LogInformation($"Fetched policy file in {stopwatch.ElapsedMilliseconds} ms.");
                }
                catch (HttpRequestException e)
                {
                    if (responseMessage != null && responseMessage.StatusCode == HttpStatusCode.NotFound ||
                        responseMessage != null && responseMessage.StatusCode == HttpStatusCode.Forbidden ||
                        e.InnerException is AuthenticationException)
                    {
                        _log.LogWarning(e,
                            $"Could not fetch policy file for {domain}");
                    }
                    else
                    {
                        _log.LogError(e, 
                            $"Could not fetch policy file for {domain}");
                    }

                    errors.Add(new FailedToFetch("SSL connection could not be established when attempting to fetch policy file.", e.InnerException != null ? e.InnerException?.Message : string.Empty));
                }
                catch (OperationCanceledException e)
                {
                    _log.LogError(e, $"Could not fetch policy file for {domain}");
                    errors.Add(new FailedToFetch("Timed Out when fetching policy file.",
                        string.Format(ErrorResources.FetchTimeOutMarkdown, url)));
                }
            }

            var result = _parser.Parse(domain, responseBody, errors);

            EvaluationResult<MtaStsPolicyResult> evaluationResult = await _evaluator.Evaluate(result);
            result.Errors.AddRange(evaluationResult.AdvisoryMessages.OfType<MtaStsAdvisoryMessage>().ToList());

            return result;
        }
    }
}