using MailCheck.MtaSts.Contracts;

namespace MailCheck.MtaSts.Evaluator.Explainers
{
    public interface ITagExplainer
    {
        void AddExplanation(MtaStsRecord record);
    }
}