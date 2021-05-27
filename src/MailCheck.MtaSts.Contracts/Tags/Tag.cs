using System;

namespace MailCheck.MtaSts.Contracts.Tags
{
    public abstract class Tag
    {
        protected Tag(string type, string rawValue)
        {
            Type = type;
            RawValue = rawValue;
        }

        public string Type { get; }
        public string RawValue { get; }
        public string Explanation { get; set; }

        public override string ToString()
        {
            return $"{nameof(RawValue)}: {RawValue}{Environment.NewLine}{nameof(Explanation)}: {Explanation}";
        }
    }
}