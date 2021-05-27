using System.Linq;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Contracts.Tags;

namespace MailCheck.MtaSts.Evaluator.Explainers
{
    public class PolicyVersionIdTagExplainer : ITagExplainer
    {
        public void AddExplanation(MtaStsRecord record)
        {
            PolicyVersionIdTag policyVersionId = record.Tags.OfType<PolicyVersionIdTag>().FirstOrDefault();

            if (policyVersionId != null)
            {
                policyVersionId.Explanation = MtaStsExplainerResource.PolicyVersionIdExplanation;
            }
        }
    }
}