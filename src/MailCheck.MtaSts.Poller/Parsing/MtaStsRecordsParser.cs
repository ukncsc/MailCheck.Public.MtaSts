using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Poller.Domain;
using MailCheck.MtaSts.Poller.Parsing.MailCheck.MtaSts.Poller.Parsing;

namespace MailCheck.MtaSts.Poller.Parsing
{
    public interface IMtaStsRecordsParser
    {
        EvaluationResult<MtaStsRecords> Parse(MtaStsRecordInfos mtaStsRecordInfos);
    }

    public class MtaStsRecordsParser : IMtaStsRecordsParser
    {
        private readonly IMtaStsRecordParser _parser;

        public MtaStsRecordsParser(IMtaStsRecordParser parser)
        {
            _parser = parser;
        }
        public EvaluationResult<MtaStsRecords> Parse(MtaStsRecordInfos mtaStsRecordInfos)
        {
            List<MtaStsRecord> records = new List<MtaStsRecord>();
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();

            if (mtaStsRecordInfos.RecordsInfos.Any())
            {
                foreach (MtaStsRecordInfo mtaStsRecordInfo in mtaStsRecordInfos.RecordsInfos)
                {
                    EvaluationResult<MtaStsRecord> mtaStsRecord = _parser.Parse(mtaStsRecordInfo);
                    records.Add(mtaStsRecord.Item);
                    errors.AddRange(mtaStsRecord.AdvisoryMessages);
                }
            }

            MtaStsRecords mtaStsRecords = new MtaStsRecords(mtaStsRecordInfos.Domain, records, mtaStsRecordInfos.MessageSize);
            return new EvaluationResult<MtaStsRecords>(mtaStsRecords, errors);
        }
    }
}