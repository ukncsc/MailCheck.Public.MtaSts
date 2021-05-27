using System.Threading.Tasks;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts;

namespace MailCheck.MtaSts.Poller.Rules
{
    public interface IMtaStsRecordsEvaluator : IEvaluator<MtaStsRecords>
    {
    }

    public class MtaStsRecordsEvaluator : IMtaStsRecordsEvaluator
    {
        private readonly IEvaluator<MtaStsRecords> _recordsEvaluator;
        private readonly IEvaluator<MtaStsRecord> _recordEvaluator;

        public MtaStsRecordsEvaluator(IEvaluator<MtaStsRecords> recordsEvaluator,
            IEvaluator<MtaStsRecord> recordEvaluator)
        {
            _recordsEvaluator = recordsEvaluator;
            _recordEvaluator = recordEvaluator;
        }

        public async Task<EvaluationResult<MtaStsRecords>> Evaluate(MtaStsRecords item)
        {
            EvaluationResult<MtaStsRecords> recordsEvaluationResult = await _recordsEvaluator.Evaluate(item);

            foreach (MtaStsRecord mtaStsRecord in item.Records)
            {
                EvaluationResult<MtaStsRecord> mtaStsRecordEvaluationResult = await _recordEvaluator.Evaluate(mtaStsRecord);
                recordsEvaluationResult.AdvisoryMessages.AddRange(mtaStsRecordEvaluationResult.AdvisoryMessages);
            }

            return recordsEvaluationResult;
        }
    }
}