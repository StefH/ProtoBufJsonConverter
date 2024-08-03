using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

namespace ProtoBufJsonConverter.Json;

internal class WellKnownTypesConverter : JsonConverter
{
    private readonly ExpandoObjectConverter _converter = new();

    public override bool CanConvert(Type objectType)
    {
        return typeof(Any).IsAssignableFrom(objectType);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        // The only way to find where this json object begins and ends is by reading it in as a generic ExpandoObject.
        // Read an entire object from the reader.
        var expandoObject = _converter.ReadJson(reader, objectType, existingValue, serializer);

        // Convert it back to json text.
        var json = JsonConvert.SerializeObject(expandoObject);

        // And let protobuf's parser parse the text.
        //var message = (IMessage)Activator.CreateInstance(objectType);

        //var x = _jsonParser.Parse(json, message.Descriptor);

        return "x";
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is Any any)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("@type");
            writer.WriteValue(any.TypeUrl);

            writer.WritePropertyName("value");
            
            any.Unpack<>()

            writer.WriteEndObject();
        }
    }
}