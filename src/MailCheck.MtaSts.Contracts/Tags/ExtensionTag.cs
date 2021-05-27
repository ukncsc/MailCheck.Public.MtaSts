namespace MailCheck.MtaSts.Contracts.Tags
{
    public class ExtensionTag : Tag
    {
        public ExtensionTag(string rawValue, string value) : base(nameof(ExtensionTag), rawValue)
        {
            Value = value;
        }

        public string Value { get; }
    }
}