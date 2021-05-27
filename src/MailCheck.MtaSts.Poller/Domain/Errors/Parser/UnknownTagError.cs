//using System;

//namespace MailCheck.MtaSts.Poller.Domain.AdvisoryMessages.Parser
//{
//    public class UnknownTagError : Error
//    {
//        private static readonly Guid _id = Guid.Parse("f5ab36d2-0bf0-4036-81c7-e69b25dd44c6");

//        public UnknownTagError(string tagKey, string tagValue) : base(_id, ErrorType.Error, FormatError(tagKey, tagValue), null)
//        {

//        }

//        private static string FormatError(string tagKey, string tagValue) => string.Format(MtaStsParserErrorMessages.UnknownItemError, tagKey, tagValue);
//    }
//}