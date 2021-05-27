using System;
using MailCheck.MtaSts.Contracts.Keys;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MailCheck.MtaSts.Contracts.Deserialisation
{
    public class KeyConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            string keyType = jo["type"].Value<string>();

            switch (keyType.ToLower())
            {
                case "maxagekey":
                    return JsonConvert.DeserializeObject<MaxAgeKey>(jo.ToString());
                case "modekey":
                    return JsonConvert.DeserializeObject<ModeKey>(jo.ToString());
                case "mxkey":
                    return JsonConvert.DeserializeObject<MxKey>(jo.ToString());
                case "versionkey":
                    return JsonConvert.DeserializeObject<VersionKey>(jo.ToString());
                case "unknownkey":
                    return JsonConvert.DeserializeObject<UnknownKey>(jo.ToString());
                default:
                    throw new InvalidOperationException($"Failed to convert type of {keyType}.");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Key);
        }
    }
}