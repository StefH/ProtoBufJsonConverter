using System.Diagnostics.CodeAnalysis;
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

            if (TryParseAsValue(value, out var parsedValue))
            {
                return parsedValue;
            }
        }

        if (typeof(Struct) == objectType)
        {
            var expandoObject = ReadAsExpandoObject(reader, objectType, existingValue, serializer);

            var @struct = new Struct();
            foreach (var field in (IDictionary<string, object?>)expandoObject[Struct.FieldName]!)
            {
                var value = TryParseAsValue(field.Value, out var parsedValue) ? parsedValue : new Value();
                @struct.Fields.Add(field.Key, value);
            }

            return @struct;
        }

        if (typeof(ListValue) == objectType)
        {
            var expandoObject = ReadAsExpandoObject(reader, objectType, existingValue, serializer);

            var listValue = new ListValue();
            foreach (var item in (List<object>)expandoObject[ListValue.FieldName]!)
            {
                var value = TryParseAsValue(item, out var parsedValue) ? parsedValue : new Value();
                listValue.Values.Add(value);
            }

            return listValue;
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

        if (value is Struct @struct)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(Struct.FieldName);

            writer.WriteStartObject();
            foreach (var field in @struct.Fields)
            {
                writer.WritePropertyName(field.Key);
                WriteJson(writer, field.Value, serializer);
            }
            writer.WriteEndObject();

            writer.WriteEndObject();
            return;
        }

        if (value is ListValue listValue)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(ListValue.FieldName);

            writer.WriteStartArray();
            foreach (var item in listValue.Values)
            {
                WriteJson(writer, item, serializer);
            }
            writer.WriteEndArray();

            writer.WriteEndObject();
            return;
        }

        if (value is Any any)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(Any.TypeUrlPropertyName);
            writer.WriteValue(any.TypeUrl);

            writer.WritePropertyName(Any.ValuePropertyName);

            var unwrappedValue = any.GetUnwrappedValue();
            writer.WriteValue(unwrappedValue);

            writer.WriteEndObject();
        }
    }

    /// <summary>
    /// The only way to find where this json object begins and ends is by reading it in as a generic ExpandoObject.
    /// Read an entire object from the reader.
    /// </summary>
    private IDictionary<string, object?> ReadAsExpandoObject(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        return (IDictionary<string, object?>)_converter.ReadJson(reader, objectType, existingValue, serializer)!;
    }

    private static bool TryParseAsValue(object? value, [NotNullWhen(true)] out Value? parsedValue)
    {
        if (value is long and 0)
        {
            parsedValue = new Value { NullValue = NullValue.NullValue };
            return true;
        }

        if (value is double numberValue)
        {
            parsedValue = new Value { NumberValue = numberValue };
            return true;
        }

        if (value is string stringValue)
        {
            parsedValue = new Value { StringValue = stringValue };
            return true;
        }

        if (value is bool boolValue)
        {
            parsedValue = new Value { BoolValue = boolValue };
            return true;
        }

        if (value is Struct structValue)
        {
            parsedValue = new Value { StructValue = structValue };
            return true;
        }

        if (value is ListValue listValue)
        {
            parsedValue = new Value { ListValue = listValue };
            return true;
        }

        parsedValue = default;
        return false;
    }
}