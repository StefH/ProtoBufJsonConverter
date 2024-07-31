extern alias gpb;
using System.Dynamic;
using gpb::Google.Protobuf;
using gpb::Google.Protobuf.Reflection;
using gpb::Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Type = System.Type;

namespace ProtoBufJsonConverter.Json
{
    internal class WellKnownTypesConverter : JsonConverter
    {
        private readonly ExpandoObjectConverter _converter = new ExpandoObjectConverter();

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Any) ||
                   objectType == typeof(Timestamp) ||
                   objectType == typeof(Duration) ||
                   objectType == typeof(FieldMask) ||
                   objectType == typeof(Struct) ||
                   objectType == typeof(Value) ||
                   objectType == typeof(ListValue) ||
                   objectType == typeof(BoolValue) ||
                   objectType == typeof(Int32Value) ||
                   objectType == typeof(Int64Value) ||
                   objectType == typeof(UInt32Value) ||
                   objectType == typeof(UInt64Value) ||
                   objectType == typeof(FloatValue) ||
                   objectType == typeof(DoubleValue) ||
                   objectType == typeof(StringValue) ||
                   objectType == typeof(BytesValue);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var typeRegistry = TypeRegistry.FromMessages(Any.Descriptor, StringValue.Descriptor);

            var str = new StringValue();
            str.Value = "stef";
            var stf  = JsonFormatter.Default.Format(str);
            //var c = StringValue.Parser.ParseJson(stf);

          //  var o = (ExpandoObject)_converter.ReadJson(reader, objectType, existingValue, serializer)!;

            // Convert it back to json text.
         //   var text = JsonConvert.SerializeObject(o);

            var ssss = reader.ReadAsString();
            return StringValue.Parser.ParseJson(ssss);

            //var any = new Any();
            ////any.TypeUrl = "type.googleapis.com/google.protobuf.StringValue";
            //any.Value = ByteString.CopyFromUtf8("stef");

            //var jsonF = new JsonFormatter(new JsonFormatter.Settings(true, typeRegistry));

            //var stef = jsonF.Format(any);

            //var x = any.Unpack<StringValue>();


            var json = "text"; // JObject.Load(reader).ToString();

            if (objectType == typeof(Any))
            {
                //var x = message.Descriptor.Parser.ParseJson(text);

                var jsonParser = new JsonParser(new JsonParser.Settings(3).WithTypeRegistry(typeRegistry));
                return jsonParser.Parse(json, Any.Descriptor);
            }
            if (objectType == typeof(Timestamp))
            {
                return Timestamp.Parser.ParseJson(json);
            }
            if (objectType == typeof(Duration))
            {
                return Duration.Parser.ParseJson(json);
            }
            if (objectType == typeof(FieldMask))
            {
                return FieldMask.Parser.ParseJson(json);
            }
            if (objectType == typeof(Struct))
            {
                return Struct.Parser.ParseJson(json);
            }
            if (objectType == typeof(Value))
            {
                return Value.Parser.ParseJson(json);
            }
            if (objectType == typeof(ListValue))
            {
                return ListValue.Parser.ParseJson(json);
            }
            if (objectType == typeof(BoolValue))
            {
                return BoolValue.Parser.ParseJson(json);
            }
            if (objectType == typeof(Int32Value))
            {
                return Int32Value.Parser.ParseJson(json);
            }
            if (objectType == typeof(Int64Value))
            {
                return Int64Value.Parser.ParseJson(json);
            }
            if (objectType == typeof(UInt32Value))
            {
                return UInt32Value.Parser.ParseJson(json);
            }
            if (objectType == typeof(UInt64Value))
            {
                return UInt64Value.Parser.ParseJson(json);
            }
            if (objectType == typeof(FloatValue))
            {
                return FloatValue.Parser.ParseJson(json);
            }
            if (objectType == typeof(DoubleValue))
            {
                return DoubleValue.Parser.ParseJson(json);
            }
            if (objectType == typeof(StringValue))
            {
                return StringValue.Parser.ParseJson(json);
            }
            if (objectType == typeof(BytesValue))
            {
                return BytesValue.Parser.ParseJson(json);
            }

            throw new JsonSerializationException($"Unexpected type: {objectType}");
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is IMessage message)
            {
                writer.WriteRawValue(JsonFormatter.Default.Format(message));
            }
        }
    }
}
