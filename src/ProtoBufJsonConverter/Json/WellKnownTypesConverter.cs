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
        return typeof(IWellKnownType).IsAssignableFrom(objectType) || objectType == typeof(NullValue);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (typeof(NullValue) == objectType)
        {
            return NullValue.NullValue;
        }

        if (typeof(Value) == objectType)
        {
            var value = _converter.ReadJson(reader, objectType, existingValue, serializer);

            if (value is long longValue && reader.TokenType == JsonToken.Integer && longValue == 0)
            {
                return new Value { NullValue = NullValue.NullValue };
            }

            if (value is double numberValue)
            {
                return new Value { NumberValue = numberValue };
            }

            if (value is string stringValue)
            {
                return new Value { StringValue = stringValue };
            }

            if (value is bool boolValue)
            {
                return new Value { BoolValue = boolValue };
            }

            if (value is Struct structValue)
            {
                return new Value { StructValue = structValue };
            }

            if (value is ListValue listValue)
            {
                return new Value { ListValue = listValue };
            }
        }

        if (typeof(Any) == objectType)
        {
            var expandoObject = ReadAsExpandoObject(reader, objectType, existingValue, serializer);

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
        if (value is NullValue nullValue)
        {
            writer.WriteValue(nullValue);
            return;
        }
        
        if (value is Value val)
        {
            if (val.Kind.Is(1))
            {
                writer.WriteValue(val.NullValue);
            }
            else if (val.Kind.Is(2))
            {
                writer.WriteValue(val.NumberValue);
            }
            else if (val.Kind.Is(3))
            {
                writer.WriteValue(val.StringValue);
            }
            else if (val.Kind.Is(4))
            {
                writer.WriteValue(val.BoolValue);
            }
            else if (val.Kind.Is(5))
            {
                serializer.Serialize(writer, val.StructValue);
            }
            else if (val.Kind.Is(6))
            {
                serializer.Serialize(writer, val.ListValue);
            }
            return;
        }
        
        if (value is Any any)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(Any.TypeUrlPropertyName);
            writer.WriteValue(any.TypeUrl);

            writer.WritePropertyName(Any.ValuePropertyName);

            var v = any.GetUnwrappedValue();
            writer.WriteValue(v);

            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// he only way to find where this json object begins and ends is by reading it in as a generic ExpandoObject.
    /// Read an entire object from the reader.
    /// </summary>
    private IDictionary<string, object?> ReadAsExpandoObject(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return (IDictionary<string, object?>)_converter.ReadJson(reader, objectType, existingValue, serializer)!;
    }
}