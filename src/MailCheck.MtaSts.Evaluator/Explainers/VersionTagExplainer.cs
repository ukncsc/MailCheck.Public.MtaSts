using System.Linq;
using MailCheck.MtaSts.Contracts;
using MailCheck.MtaSts.Contracts.Tags;

namespace MailCheck.MtaSts.Evaluator.Explainers
{
    public class VersionTagExplainer : ITagExplainer
    {
        public void AddExplanation(MtaStsRecord record)
        {
            VersionTag version = record.Tags.OfType<VersionTag>().FirstOrDefault();

            if (version != null)
            {
                version.Explanation = MtaStsExplainerResource.VersionExplanation;
            }
        }
    }
}