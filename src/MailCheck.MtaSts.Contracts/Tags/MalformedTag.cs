namespace MailCheck.MtaSts.Contracts.Tags
{
    public class MalformedTag : Tag
    {
        public MalformedTag(string rawValue) : base(nameof(MalformedTag), rawValue)
        {

        }
    }
}