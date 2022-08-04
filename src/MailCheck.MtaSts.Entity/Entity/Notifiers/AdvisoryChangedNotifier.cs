using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Entity.Config;
using MailCheck.MtaSts.Entity.Entity.Notifications;
using Microsoft.Extensions.Logging;
using AdvisoryMessage = MailCheck.Common.Contracts.Advisories.AdvisoryMessage;

namespace MailCheck.MtaSts.Entity.Entity.Notifiers
{
    public class AdvisoryChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IMtaStsEntityConfig _mtaStsEntityConfig;
        private readonly IEqualityComparer<AdvisoryMessage> _messageEqualityComparer;
        private readonly ILogger<AdvisoryChangedNotifier> _log;

        public AdvisoryChangedNotifier(IMessageDispatcher dispatcher, IMtaStsEntityConfig mtaStsEntityConfig,
            IEqualityComparer<AdvisoryMessage> messageEqualityComparer, ILogger<AdvisoryChangedNotifier> log)
        {
            _dispatcher = dispatcher;
            _mtaStsEntityConfig = mtaStsEntityConfig;
            _messageEqualityComparer = messageEqualityComparer;
            _log = log;
        }

        public void Handle(string domain, IEnumerable<AdvisoryMessage> currentMessages, IEnumerable<AdvisoryMessage> newMessages)
        {
            currentMessages = currentMessages ?? new List<AdvisoryMessage>();
            newMessages = newMessages ?? new List<AdvisoryMessage>();
            
            List<AdvisoryMessage> addedMessages =
                newMessages.Except(currentMessages, _messageEqualityComparer).ToList();
            if (addedMessages.Any())
            {
                MtaStsAdvisoryAdded advisoryAdded = new MtaStsAdvisoryAdded(
                    domain,
                    addedMessages);
                _dispatcher.Dispatch(advisoryAdded, _mtaStsEntityConfig.SnsTopicArn);
            }

            List<AdvisoryMessage> removedMessages =
                currentMessages.Except(newMessages, _messageEqualityComparer).ToList();
            if (removedMessages.Any())
            {
                MtaStsAdvisoryRemoved advisoryRemoved = new MtaStsAdvisoryRemoved(
                    domain,
                    removedMessages);
                _dispatcher.Dispatch(advisoryRemoved, _mtaStsEntityConfig.SnsTopicArn);
            }

            List<AdvisoryMessage> sustainedMessages =
                currentMessages.Intersect(newMessages, _messageEqualityComparer).ToList();
            if (sustainedMessages.Any())
            {
                MtaStsAdvisorySustained advisorySustained = new MtaStsAdvisorySustained(
                    domain,
                    sustainedMessages);
                _dispatcher.Dispatch(advisorySustained, _mtaStsEntityConfig.SnsTopicArn);
            }
        }
    }
}