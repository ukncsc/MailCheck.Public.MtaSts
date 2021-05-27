using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Advisories;
using MailCheck.Common.Processors.Evaluators;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Evaluator.Explainers;

namespace MailCheck.MtaSts.Evaluator
{
    public interface IMtaStsEvaluationProcessor
    {
        Task<List<AdvisoryMessage>> Process(MtaStsRecords mtaStsRecords);
    }

    public class MtaStsEvaluationProcessor: IMtaStsEvaluationProcessor{

        private readonly IEvaluator<MtaStsRecord> _evaluator;
        private readonly IMtaStsRecordExplainer _recordExplainer;

        public MtaStsEvaluationProcessor(IEvaluator<MtaStsRecord> evaluator, IMtaStsRecordExplainer recordExplainer)
        {
            _evaluator = evaluator;
            _recordExplainer = recordExplainer;
        }

        public async Task<List<AdvisoryMessage>> Process(MtaStsRecords mtaStsRecords)
        {
            List<AdvisoryMessage> errors = new List<AdvisoryMessage>();

            if (mtaStsRecords == null) return errors;

            foreach (MtaStsRecord mtaStsRecord in mtaStsRecords.Records)
            {
                _recordExplainer.Process(mtaStsRecord);

                EvaluationResult<MtaStsRecord> evaluationResult = await _evaluator.Evaluate(mtaStsRecord);
                errors.AddRange(evaluationResult.AdvisoryMessages);
            }

            return errors;
        }
    }
}