// https://andrewlock.net/disambiguating-types-with-the-same-name-with-extern-alias/
extern alias gpb;

using Google.Protobuf.Reflection;
using gpb::Google.Protobuf;
using gpb::Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using TypeRegistry = gpb::Google.Protobuf.Reflection.TypeRegistry;

namespace ProtoBufJsonConverter
{
    extern alias gpb;

    /// <summary>
    /// Lets Newtonsoft.Json and Protobuf's json converters play nicely together.
    /// The default Newtonsoft.Json Deserialize method will not correctly deserialize proto messages.
    /// </summary>
    internal class ProtoMessageConverter : JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            return typeof(gpb::Google.Protobuf.IMessage).IsAssignableFrom(objectType);
        }

        public override object? ReadJson(JsonReader reader, System.Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // The only way to find where this json object begins and ends is by reading it in as a generic ExpandoObject.
            // Read an entire object from the reader.
            var converter = new ExpandoObjectConverter();
            var o = converter.ReadJson(reader, objectType, existingValue, serializer);

            // Convert it back to json text.
            var text = JsonConvert.SerializeObject(o);

            var stringValue = new StringValue { Value = "stef" };
            var anyMessage = Any.Pack(stringValue);

            byte[] byteArray = anyMessage.ToByteArray();
            var b64 = System.Convert.ToBase64String(byteArray);

            // Create a type registry and register the StringValue type
            var typeRegistry = TypeRegistry.FromMessages(StringValue.Descriptor);

            // Convert the Any message to JSON using the type registry
            var jsonFormatter = new JsonFormatter(new JsonFormatter.Settings(true).WithTypeRegistry(typeRegistry));


            // Convert to JSON
            string json = jsonFormatter.Format(anyMessage);


            // And let protobuf's parser parse the text.
            var message = (gpb::Google.Protobuf.IMessage)Activator.CreateInstance(objectType);

            //var x = message.Descriptor.Parser.ParseJson(text);

            var JsonParser = new JsonParser(new JsonParser.Settings(3).WithTypeRegistry(typeRegistry));

            return JsonParser.Parse(text, message.Descriptor);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            // Let Protobuf's JsonFormatter do all the work.
            writer.WriteRawValue(gpb::Google.Protobuf.JsonFormatter.Default.Format((IMessage?)value));
        }
    }
}
