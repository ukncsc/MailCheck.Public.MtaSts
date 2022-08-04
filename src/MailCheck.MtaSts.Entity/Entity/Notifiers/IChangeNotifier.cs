using System.Collections.Generic;
using MailCheck.Common.Contracts.Advisories;

namespace MailCheck.MtaSts.Entity.Entity.Notifiers
{
    public interface IChangeNotifier
    {
        void Handle(string domain, IEnumerable<AdvisoryMessage> currentMessages, IEnumerable<AdvisoryMessage> newMessages);
    }
}