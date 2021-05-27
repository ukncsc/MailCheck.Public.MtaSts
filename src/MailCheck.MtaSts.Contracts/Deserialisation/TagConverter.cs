using System;
using MailCheck.MtaSts.Contracts.Tags;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailCheck.MtaSts.Contracts.Deserialisation
{
    public class TagConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            string tagType = jo["type"].Value<string>();

            switch (tagType.ToLower())
            {
                case "policyversionidtag":
                    return JsonConvert.DeserializeObject<PolicyVersionIdTag>(jo.ToString());
                case "versiontag":
                    return JsonConvert.DeserializeObject<VersionTag>(jo.ToString());
                case "extensiontag":
                    return JsonConvert.DeserializeObject<ExtensionTag>(jo.ToString());
                default:
                    throw new InvalidOperationException($"Failed to convert type of {tagType}.");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Tag);
        }
    }
}