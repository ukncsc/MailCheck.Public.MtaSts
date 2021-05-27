using System;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Poller.Dns;
using MailCheck.MtaSts.Poller.Domain;
using MailCheck.MtaSts.Poller.Parsing;
using MailCheck.MtaSts.Poller.Rules;
using Microsoft.Extensions.Logging;

namespace MailCheck.MtaSts.Poller
{
    public interface IMtaStsProcessor
    {
        Task<MtaStsPollResult> Process(string domain);
    }

    public class MtaStsProcessor : IMtaStsProcessor
    {
        private readonly IDnsClient _dnsClient;
        private readonly IMtaStsRecordsParser _parser;
        private readonly IMtaStsRecordsEvaluator _evaluator;
        private readonly ILogger<MtaStsProcessor> _log;

        public MtaStsProcessor(IDnsClient dnsClient,
            IMtaStsRecordsParser parser,
            IMtaStsRecordsEvaluator evaluator,
            ILogger<MtaStsProcessor> log)
        {
            _dnsClient = dnsClient;
            _parser = parser;
            _evaluator = evaluator;
            _log = log;
        }

        public async Task<MtaStsPollResult> Process(string domain)
        {
            MtaStsRecordInfos mtaStsRecordInfos = await _dnsClient.GetMtaStsRecords(domain);

            if (mtaStsRecordInfos.HasError)
            {
                string message = $"Failed MTA-STS record query for {domain} with error {mtaStsRecordInfos.AdvisoryMessage.Text}";
                _log.LogError($"{message} {Environment.NewLine} Audit Trail: {mtaStsRecordInfos.AuditTrail}");
                return new MtaStsPollResult(domain, mtaStsRecordInfos.AdvisoryMessage);
            }

            if (mtaStsRecordInfos.RecordsInfos.Count == 0 || mtaStsRecordInfos.RecordsInfos.TrueForAll(x => string.IsNullOrWhiteSpace(x.Record)))
            {
                _log.LogInformation($"MTA STS records missing or empty for {domain}, Name server: {mtaStsRecordInfos.NameServer}");
            }

            EvaluationResult<MtaStsRecords> parsingResult = _parser.Parse(mtaStsRecordInfos);

            EvaluationResult<MtaStsRecords> evaluationResult = await _evaluator.Evaluate(parsingResult.Item);

            return new MtaStsPollResult(evaluationResult.Item, parsingResult.AdvisoryMessages.Concat(evaluationResult.AdvisoryMessages).ToList());
        }
    }
}