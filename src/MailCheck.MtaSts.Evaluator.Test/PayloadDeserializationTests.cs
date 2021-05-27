using MailCheck.MtaSts.Contracts.Messages;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MailCheck.MtaSts.Evaluator.Test
{
    [TestFixture]
    public class PayloadDeserializationTests
    {
        private const string Payload = "{\"mtaStsRecords\":{\"domain\":\"tyneandwearltp.gov.uk\",\"records\":[{\"domain\":\"tyneandwearltp.gov.uk\",\"record\":\"v=STSv1;id=123456789;\",\"recordsParts\":[\"v=STSv1;p=reject;id=123456789\"],\"tags\":[{\"value\":\"STSv1\",\"type\":\"VersionTag\",\"rawValue\":\"v=STSv1\",\"explanation\":null},{\"value\":\"reject\",\"type\":\"ExtensionTag\",\"rawValue\":\"p=reject\",\"explanation\":null},{\"value\":\"123456789\",\"type\":\"PolicyVersionIdTag\",\"rawValue\":\"id=123456789\",\"explanation\":null}]},{\"domain\":\"tyneandwearltp.gov.uk\",\"record\":\"v=spf1 ?all\",\"recordsParts\":[\"v=spf1 ?all\"],\"tags\":[{\"value\":\"spf1 ?all\",\"type\":\"VersionTag\",\"rawValue\":\"v=spf1 ?all\",\"explanation\":null}]}],\"messageSize\":155},\"advisoryMessages\":[],\"id\":\"tyneandwearltp.gov.uk\",\"correlationId\":null,\"causationId\":\"ba4f2374-ef52-4f24-b7a1-c80b0ffbbad8\",\"type\":null,\"messageId\":null,\"timestamp\":\"0001-01-01T00:00:00\"}";

        [Test]
        public void CanDeserialise()
        {
            MtaStsRecordsPolled mtaStsRecordsPolled = JsonConvert.DeserializeObject<MtaStsRecordsPolled>(Payload, SerialisationConfig.Settings);

            Assert.That(mtaStsRecordsPolled, Is.Not.Null);
        }
    }
}