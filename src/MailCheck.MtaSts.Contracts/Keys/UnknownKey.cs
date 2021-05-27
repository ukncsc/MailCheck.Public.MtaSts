namespace MailCheck.MtaSts.Contracts.Keys
{
    public class UnknownKey : Key
    {
        public UnknownKey(string value, string rawValue) : base(nameof(UnknownKey), value, rawValue)
        {
        }
    }
}