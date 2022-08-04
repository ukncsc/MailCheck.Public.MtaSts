using System;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.PolicyFetcher.Config;
using MailCheck.MtaSts.Contracts.PolicyFetcher;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MailCheck.MtaSts.PolicyFetcher
{
    public class PolicyFetcherHandler : IHandle<MtaStsFetchPolicy>
    {
        private readonly IPolicyFetcher _policyFetcher;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IMtaStsPolicyFetcherConfig _config;
        private readonly ILogger<PolicyFetcherHandler> _log;

        public PolicyFetcherHandler(IPolicyFetcher policyFetcher,
            IMessageDispatcher dispatcher,
            IMtaStsPolicyFetcherConfig config,
            ILogger<PolicyFetcherHandler> log)
        {
            _policyFetcher = policyFetcher;
            _dispatcher = dispatcher;
            _config = config;
            _log = log;
        }

        public async Task Handle(MtaStsFetchPolicy message)
        {
            try
            {
                MtaStsPolicyResult mtaStsPolicyResult = await _policyFetcher.Process(message.Id);

                MtaStsPolicyFetched mtaStsPolicyFetched = new MtaStsPolicyFetched(message.Id, mtaStsPolicyResult);

                _dispatcher.Dispatch(mtaStsPolicyFetched, _config.SnsTopicArn);
                _log.LogInformation(JsonConvert.SerializeObject(mtaStsPolicyFetched));
            }
            catch (Exception e)
            {
                string error = $"Error occurred fetching policy file for domain {message.Id}";
                _log.LogError(e, error);
                throw;
            }
        }
    }
}