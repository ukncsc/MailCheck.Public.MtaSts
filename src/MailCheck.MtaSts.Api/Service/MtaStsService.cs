using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Api.Domain;
using MailCheck.MtaSts.Api.Config;
using MailCheck.MtaSts.Api.Dao;
using Microsoft.Extensions.Logging;

namespace MailCheck.MtaSts.Api.Service
{
    public interface IMtaStsService
    {
        Task<MtaStsInfoResponse> GetMtaStsForDomain(string requestDomain);
    }

    public class MtaStsService : IMtaStsService
    {
        private readonly IMtaStsApiDao _dao;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IMtaStsApiConfig _config;
        private readonly ILogger<MtaStsService> _log;

        public MtaStsService(IMessagePublisher messagePublisher, IMtaStsApiDao dao, IMtaStsApiConfig config, ILogger<MtaStsService> log)
        {
            _messagePublisher = messagePublisher;
            _dao = dao;
            _config = config;
            _log = log;
        }

        public async Task<MtaStsInfoResponse> GetMtaStsForDomain(string requestDomain)
        {
            MtaStsInfoResponse response = await _dao.GetMtaStsForDomain(requestDomain);

            if (response is null)
            {
                _log.LogInformation($"MtaSts entity state does not exist for domain {requestDomain} - publishing DomainMissing");
                await _messagePublisher.Publish(new DomainMissing(requestDomain), _config.MicroserviceOutputSnsTopicArn);
            }

            return response;
        }
    }
}
