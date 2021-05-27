namespace MailCheck.MtaSts.Contracts.Tags
{
    public class PolicyVersionIdTag : Tag
    {
        public PolicyVersionIdTag(string rawValue, string value) : base(nameof(PolicyVersionIdTag), rawValue)
        {
            Value = value;
        }

        public string Value { get; }
    }
}