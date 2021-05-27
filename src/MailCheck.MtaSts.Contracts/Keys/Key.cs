using System;
namespace MailCheck.MtaSts.Contracts.Keys
{
    public abstract class Key
    {
        protected Key(string type, string value, string rawValue)
        {
            Type = type;
            Value = value;
            RawValue = rawValue;
        }

        public string Type { get; }
        public string Value { get; }
        public string RawValue { get; }
        public string Explanation { get; set; }
    }
}
