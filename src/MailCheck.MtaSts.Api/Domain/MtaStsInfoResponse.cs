using System;
using System.Collections.Generic;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.MtaSts.Contracts;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.MtaSts.Contracts.Entity;
using MailCheck.MtaSts.Contracts.PolicyFetcher;

namespace MailCheck.MtaSts.Api.Domain
{
    public class MtaStsInfoResponse
    {
        public MtaStsInfoResponse(string id, MtaStsState mtaStsState, MtaStsPolicyResult policy = null, MtaStsRecords mtaStsRecords = null, List<AdvisoryMessage> messages = null, DateTime? lastUpdated = null)
        {
            Id = id;
            Status = mtaStsState;
            Policy = policy;
            MtaStsRecords = mtaStsRecords;
            Messages = messages;
            LastUpdated = lastUpdated;
        }

        public string Id { get; }

        public MtaStsState Status { get; }

        public MtaStsRecords MtaStsRecords { get; }

        public MtaStsPolicyResult Policy { get; }

        public List<AdvisoryMessage> Messages { get; }

        public DateTime? LastUpdated { get; }
    }
}
