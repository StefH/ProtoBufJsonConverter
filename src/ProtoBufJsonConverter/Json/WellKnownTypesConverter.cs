extern alias gpb;
using gpb::Google.Protobuf;
using gpb::Google.Protobuf.Reflection;
using gpb::Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Type = System.Type;

namespace ProtoBufJsonConverter.Json;

internal class WellKnownTypesConverter : JsonConverter
{
    private readonly TypeRegistry _typeRegistry = TypeRegistry.FromMessages(
    [
        StringValue.Descriptor,
        Int32Value.Descriptor
    ]);
    private readonly JsonParser _jsonParser;
    private readonly JsonFormatter _jsonFormatter;
    private readonly ExpandoObjectConverter _converter = new();

    public WellKnownTypesConverter()
    {
        _jsonParser = new JsonParser(new JsonParser.Settings(99, _typeRegistry));
        _jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true, _typeRegistry));
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(IMessage).IsAssignableFrom(objectType); // || objectType == typeof(ByteString);
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
        var message = (IMessage)Activator.CreateInstance(objectType);

        var x = _jsonParser.Parse(json, message.Descriptor);

        return x;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is IMessage message)
        {
            writer.WriteRawValue(_jsonFormatter.Format(message));
        }
    }
}