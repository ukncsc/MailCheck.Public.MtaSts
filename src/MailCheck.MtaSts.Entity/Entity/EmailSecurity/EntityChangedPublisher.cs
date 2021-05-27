using System;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Entity.Config;
using Microsoft.Extensions.Logging;

namespace MailCheck.MtaSts.Entity.Entity.EmailSecurity
{
    public interface IEntityChangedPublisher
    {
        void Publish(string domain, MtaStsEntityState state, string reason);
    }

    public class EntityChangedPublisher : IEntityChangedPublisher
    {
        private readonly IMtaStsEntityConfig _mtaStsEntityConfig;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ILogger<EntityChangedPublisher> _log;

        public EntityChangedPublisher (
            IMtaStsEntityConfig config,
            IMessageDispatcher dispatcher,
            ILogger<EntityChangedPublisher> log)
        {
            _mtaStsEntityConfig = config;
            _dispatcher = dispatcher;
            _log = log;
        }

        public void Publish(string domain, MtaStsEntityState state, string reason)
        {
            
            EntityChanged message = new EntityChanged(domain)
            {
                RecordType = _mtaStsEntityConfig.RecordType,
                NewEntityDetail = state,
                ChangedAt = DateTime.UtcNow,
                ReasonForChange = reason
            };

            _dispatcher.Dispatch(message, _mtaStsEntityConfig.SnsTopicArn);
            _log.LogInformation($"EntityChanged message dispatched for ${domain}");
        }
    }
}
