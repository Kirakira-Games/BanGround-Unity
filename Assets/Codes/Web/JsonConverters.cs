namespace Web
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;

    public class JsonWithTimeStamps
    {
        [JsonProperty("updatedAt")]
        [JsonConverter(typeof(ISODateToStringConverter))]
        public DateTime UpdatedAt;

        [JsonProperty("createdAt")]
        [JsonConverter(typeof(ISODateToStringConverter))]
        public DateTime CreatedAt;
    }

    /// <summary>
    /// Converter to convert 64 bit integers to a string during serialization and convert it back
    /// during deserialization.
    /// </summary>
    public class LongToStringConverter : JsonConverter
    {
        public override bool CanWrite => true;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(long));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            return token.ToObject<long>();
        }
    }

    /// <summary>
    /// Converter to convert DateTime to an ISO string during serialization and convert it back
    /// during deserialization.
    /// </summary>
    public class ISODateToStringConverter : JsonConverter
    {
        public override bool CanWrite => true;
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(DateTime));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((DateTime)value).ToString("O"));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            return DateTime.Parse(token.ToObject<string>());
        }
    }
}