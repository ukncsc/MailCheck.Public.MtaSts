using System;
using System.Collections.Generic;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Contracts.Entity;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.PolicyFetcher;
using MailCheck.MtaSts.Contracts.Messages;

namespace MailCheck.MtaSts.Entity.Entity
{
    public class MtaStsEntityState
    {
        public virtual string Id { get; }
        public virtual int Version { get; set; }
        public virtual MtaStsState MtaStsState { get; set; }
        public virtual DateTime Created { get; }
        public virtual MtaStsRecords MtaStsRecords { get; set; }
        public virtual List<MtaStsAdvisoryMessage> Messages { get; set; }
        public virtual DateTime? LastUpdated { get; set; }
        public virtual MtaStsPolicyResult Policy { get; set; }

        public MtaStsEntityState(string id, int version, MtaStsState mtaStsState, DateTime created)
        {
            Id = id;
            Version = version;
            MtaStsState = mtaStsState;
            Created = created;
            Messages = new List<MtaStsAdvisoryMessage>();
            Policy = null;
        }
    }
}
