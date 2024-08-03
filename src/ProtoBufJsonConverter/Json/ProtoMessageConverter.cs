extern alias gpb;
using System.Dynamic;
using gpb::Google.Protobuf;
using gpb::Google.Protobuf.Reflection;
using gpb::Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace ProtoBufJsonConverter.Json;

/// <summary>
/// Lets Newtonsoft.Json and Protobuf's json converters play nicely together.
/// The default Newtonsoft.Json Deserialize method will not correctly deserialize proto messages.
/// </summary>
internal class ProtoMessageConverter2 : JsonConverter
{
    private readonly ExpandoObjectConverter _converter = new ExpandoObjectConverter();

    public override bool CanConvert(System.Type objectType)
    {
        return false; // typeof(IMessage).IsAssignableFrom(objectType);
    }

    public override object? ReadJson(JsonReader reader, System.Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var json = JObject.Load(reader).ToString();

        if (objectType == typeof(Any))
        {
            return Any.Parser.ParseJson(json);
        }

        var message = (IMessage)Activator.CreateInstance(objectType);
        var typeRegistry = TypeRegistry.FromMessages(message.Descriptor);
        var jsonParser = new JsonParser(new JsonParser.Settings(3).WithTypeRegistry(typeRegistry));

        return jsonParser.Parse(json, message.Descriptor);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is IMessage message)
        {
            writer.WriteRawValue(JsonFormatter.Default.Format(message));
        }
    }

    public object? ReadJson2(JsonReader reader, System.Type objectType, object? existingValue, JsonSerializer serializer)
    {
        // The only way to find where this json object begins and ends is by reading it in as a generic ExpandoObject.
        // Read an entire object from the reader.
        
        var o = (ExpandoObject) _converter.ReadJson(reader, objectType, existingValue, serializer)!;

        // Convert it back to json text.
        var text = JsonConvert.SerializeObject(o);

        if (objectType == typeof(Any))
        {
            return Any.Parser.ParseJson(text);
        }

        //var instance = JsonConvert.DeserializeObject(text, objectType, new JsonSerializerSettings
        //{
        //    Converters = [this]
        //});

        

            

        //var stringValue = new StringValue { Value = "stef" };
        //var anyMessage = Any.Pack(stringValue);

        //byte[] byteArray = anyMessage.ToByteArray();
        //var b64 = Convert.ToBase64String(byteArray);

        // Create a type registry and register the StringValue type


        // Convert the Any message to JSON using the type registry
        //var jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true).WithTypeRegistry(typeRegistry));


        // Convert to JSON
        // string json = jsonFormatter.Format(anyMessage);


        // And let protobuf's parser parse the text.
        var message = (IMessage)Activator.CreateInstance(objectType);


        //var xxx = JsonParser.Default.Parse(text, message.Descriptor);

        var typeRegistry = TypeRegistry.FromMessages(message.Descriptor, StringValue.Descriptor);

        //var x = message.Descriptor.Parser.ParseJson(text);

        var jsonParser = new JsonParser(new JsonParser.Settings(3).WithTypeRegistry(typeRegistry));

        var x = jsonParser.Parse(text, message.Descriptor);

        return x;
    }

    public  void WriteJson3(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        // Let Protobuf's JsonFormatter do all the work.
        writer.WriteRawValue(JsonFormatter.Default.Format((IMessage?)value));
    }
}