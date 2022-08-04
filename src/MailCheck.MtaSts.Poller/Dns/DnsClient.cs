using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using MailCheck.Common.Util;
using MailCheck.MtaSts.Poller.Domain;
using MailCheck.MtaSts.Poller.Domain.Errors.Parser;
using Microsoft.Extensions.Logging;

namespace MailCheck.MtaSts.Poller.Dns
{
    public interface IDnsClient
    {
        Task<MtaStsRecordInfos> GetMtaStsRecords(string domain);
    }

    public class DnsClient : IDnsClient
    {
        private const string NonExistentDomainError = "Non-Existent Domain";
        private const string ServerFailureError = "Server Failure";
        private readonly ILookupClient _lookupClient;
        private readonly ILogger<DnsClient> _log;

        public DnsClient(ILookupClient lookupClient,
            ILogger<DnsClient> log)
        {
            _lookupClient = lookupClient;
            _log = log;
        }

        public async Task<MtaStsRecordInfos> GetMtaStsRecords(string domain)
        {
            string queryText = $"_mta-sts.{domain}";
            QueryType queryType = QueryType.TXT;

            try
            {
                IDnsQueryResponse response = await _lookupClient.QueryAsync(queryText, queryType);

                List<MtaStsRecordInfo> mtaStsRecordInfos = response.Answers.OfType<TxtRecord>()
                    .Where(x => x.Text.FirstOrDefault()?.StartsWith("v=STSv", StringComparison.OrdinalIgnoreCase) ?? false)
                    .Select(x => new MtaStsRecordInfo(domain, x.Text.Select(r => r.Escape()).ToList()))
                    .ToList();

                if (response.HasError && response.ErrorMessage != NonExistentDomainError && response.ErrorMessage != ServerFailureError)
                {
                    return new MtaStsRecordInfos(domain, new FailedPollError(response.ErrorMessage), response.MessageSize, response.NameServer?.ToString(), response.AuditTrail);
                }

                return new MtaStsRecordInfos(domain, mtaStsRecordInfos, response.MessageSize);
            }
            catch(DnsResponseException dnsException) when (dnsException.Code == DnsResponseCode.NotExistentDomain || dnsException.Code == DnsResponseCode.ServerFailure)
            {
                return new MtaStsRecordInfos(domain, new List<MtaStsRecordInfo>(), 0);
            }
        }
    }
}