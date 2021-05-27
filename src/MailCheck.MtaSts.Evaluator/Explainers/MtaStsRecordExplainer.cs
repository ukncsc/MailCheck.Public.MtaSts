using System.Collections.Generic;
using MailCheck.MtaSts.Contracts;

namespace MailCheck.MtaSts.Evaluator.Explainers
{
    public interface IMtaStsRecordExplainer
    {
        void Process(MtaStsRecord record);
    }

    public class MtaStsRecordExplainer : IMtaStsRecordExplainer
    {
        private readonly IEnumerable<ITagExplainer> _tagExplainers;

        public MtaStsRecordExplainer(IEnumerable<ITagExplainer> tagExplainers)
        {
            _tagExplainers = tagExplainers;
        }

        public void Process(MtaStsRecord record)
        {
            foreach (ITagExplainer tagExplainer in _tagExplainers)
            {
                tagExplainer.AddExplanation(record);
            }
        }
    }
}
