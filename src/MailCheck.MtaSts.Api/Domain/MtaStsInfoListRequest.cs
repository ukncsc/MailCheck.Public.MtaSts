using System.Collections.Generic;

namespace MailCheck.MtaSts.Api.Domain
{
    public class MtaStsInfoListRequest
    {
        public MtaStsInfoListRequest()
        {
            Domains = new List<string>();
        }

        public List<string> Domains { get; set; }
    }
}
