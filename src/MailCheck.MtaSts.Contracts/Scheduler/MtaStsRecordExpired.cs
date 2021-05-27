using System;
using System.Collections.Generic;
using System.Text;
using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.MtaSts.Contracts.Scheduler
{
    public class MtaStsRecordExpired : Message
    {
        public MtaStsRecordExpired(string id)
            : base(id) { }
    }
}
