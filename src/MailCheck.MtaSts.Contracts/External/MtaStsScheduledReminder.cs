using MailCheck.Common.Contracts.Messaging;

namespace MailCheck.MtaSts.Contracts.External
{
    public class MtaStsScheduledReminder : ScheduledReminder
    {
        public MtaStsScheduledReminder(string id, string resourceId)
            : base(id, resourceId)
        {
        }
    }
}
