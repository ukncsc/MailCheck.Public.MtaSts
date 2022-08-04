using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.Entity.Entity.Notifiers
{
    public interface IChangeNotifiersComposite : IChangeNotifier
    {
    }

    public class ChangeNotifiersComposite : IChangeNotifiersComposite
    {
        private readonly IEnumerable<IChangeNotifier> _notifiers;

        public ChangeNotifiersComposite(IEnumerable<IChangeNotifier> notifiers)
        {
            _notifiers = notifiers;
        }

        public void Handle(string domain, IEnumerable<AdvisoryMessage> currentMessages, IEnumerable<AdvisoryMessage> newMessages)
        {
            foreach (IChangeNotifier changeNotifier in _notifiers)
            {
                changeNotifier.Handle(domain, currentMessages, newMessages);
            }
        }
    }
}
