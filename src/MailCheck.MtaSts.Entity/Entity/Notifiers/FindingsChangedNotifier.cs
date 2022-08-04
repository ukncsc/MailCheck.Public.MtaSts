using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Contracts.Findings;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Notifiers;
using MailCheck.MtaSts.Contracts.Messages;
using MailCheck.MtaSts.Entity.Config;
using Microsoft.Extensions.Logging;

namespace MailCheck.MtaSts.Entity.Entity.Notifiers
{
    public class FindingsChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IMtaStsEntityConfig _mtaStsEntityConfig;
        private readonly IFindingsChangedNotifier _findingsChangedCalculator;
        private readonly ILogger<FindingsChangedNotifier> _log;

        public FindingsChangedNotifier(IMessageDispatcher dispatcher, IMtaStsEntityConfig mtaStsEntityConfig,
            IFindingsChangedNotifier findingsChangedCalculator, ILogger<FindingsChangedNotifier> log)
        {
            _dispatcher = dispatcher;
            _mtaStsEntityConfig = mtaStsEntityConfig;
            _findingsChangedCalculator = findingsChangedCalculator;
            _log = log;
        }

        public void Handle(string domain, IEnumerable<AdvisoryMessage> currentMessages, IEnumerable<AdvisoryMessage> newMessages)
        {
            FindingsChanged findingsChanged = _findingsChangedCalculator.Process(domain, "MTA-STS",
                ExtractFindingsFromMessages(domain, currentMessages?.OfType<MtaStsAdvisoryMessage>().ToList() ?? new List<MtaStsAdvisoryMessage>()),
                ExtractFindingsFromMessages(domain, newMessages?.OfType<MtaStsAdvisoryMessage>().ToList() ?? new List<MtaStsAdvisoryMessage>()));

            if(findingsChanged.Added?.Count > 0 || findingsChanged.Sustained?.Count > 0 || findingsChanged.Removed?.Count > 0)
            {
                _log.LogInformation($"Dispatching FindingsChanged for {domain}: {findingsChanged.Added?.Count} findings added, {findingsChanged.Sustained?.Count} findings sustained, {findingsChanged.Removed?.Count} findings removed");
                _dispatcher.Dispatch(findingsChanged, _mtaStsEntityConfig.SnsTopicArn);
            }
            else
            {
                _log.LogInformation($"No Findings to dispatch for {domain}");
            }
        }

        private List<Finding> ExtractFindingsFromMessages(string domain, List<MtaStsAdvisoryMessage> rootMessages)
        {
            List<Finding> findings = rootMessages.Select(msg => new Finding
            {
                Name = msg.Name,
                SourceUrl = $"https://{_mtaStsEntityConfig.WebUrl}/app/domain-security/{domain}/mta-sts",
                Title = msg.Text,
                EntityUri = $"domain:{domain}",
                Severity = AdvisoryMessageTypeToFindingSeverityMapping[msg.MessageType]
            }).ToList();

            return findings;
        }

        internal static readonly Dictionary<MessageType, string> AdvisoryMessageTypeToFindingSeverityMapping = new Dictionary<MessageType, string>
        {
            [MessageType.info] = "Informational",
            [MessageType.warning] = "Advisory",
            [MessageType.error] = "Urgent",
        };
    }
}
