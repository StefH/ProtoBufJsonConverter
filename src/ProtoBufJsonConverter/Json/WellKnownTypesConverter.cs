using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf;
using ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;
using ProtoBufJsonConverter.Utils;

namespace ProtoBufJsonConverter.Json;

internal class WellKnownTypesConverter : JsonConverter
{
    private const string TypeUrlPropertyName = "@type";
    private const string ValuePropertyName = "value";

    private readonly ExpandoObjectConverter _converter = new();

    public override bool CanConvert(Type objectType)
    {
        return typeof(IWellKnownType).IsAssignableFrom(objectType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        if (typeof(Any) == objectType)
        {
            // The only way to find where this json object begins and ends is by reading it in as a generic ExpandoObject.
            // Read an entire object from the reader.
            var expandoObject = (IDictionary<string, object?>) _converter.ReadJson(reader, objectType, existingValue, serializer)!;

            var typeUrl = (string) expandoObject[TypeUrlPropertyName]!;
            var value = expandoObject[ValuePropertyName];
            var bytes = SerializeUtils.Serialize(value);

            return new Any
            {
                TypeUrl = typeUrl,
                Value = new ByteString(bytes)
            };
        }

        return null;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is Any any)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(TypeUrlPropertyName);
            writer.WriteValue(any.TypeUrl);

            writer.WritePropertyName(ValuePropertyName);

            var v = any.GetUnderlyingValue();
            writer.WriteValue(v);

            writer.WriteEndObject();
        }
        else
        {
            int x = 9;
        }
    }
}