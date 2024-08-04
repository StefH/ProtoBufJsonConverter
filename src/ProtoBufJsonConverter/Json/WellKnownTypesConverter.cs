using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBufJsonConverter.Utils;

namespace ProtoBufJsonConverter.Json;

internal class WellKnownTypesConverter : JsonConverter
{
    private readonly ExpandoObjectConverter _converter = new();

    public override bool CanConvert(Type objectType)
    {
        // Support Any and NullValue
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
            var expandoObject = (IDictionary<string, object?>)_converter.ReadJson(reader, objectType, existingValue, serializer)!;

            var typeUrl = (string)expandoObject[Any.TypeUrlPropertyName]!;
            var value = expandoObject[Any.ValuePropertyName];
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

            writer.WritePropertyName(Any.TypeUrlPropertyName);
            writer.WriteValue(any.TypeUrl);

            writer.WritePropertyName(Any.ValuePropertyName);

            var v = any.GetUnderlyingValue();
            writer.WriteValue(v);

            writer.WriteEndObject();
        }
        else if (value is NullValue)
        {
            writer.WriteNull();
        }
    }
}