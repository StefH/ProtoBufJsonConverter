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
            return ParseAsValue(value);
        }

        if (typeof(Struct) == objectType)
        {
            var expandoObject = ReadAsExpandoObject(reader, objectType, existingValue, serializer);
            return ParseAsStruct(expandoObject);
        }

        if (typeof(ListValue) == objectType)
        {
            var expandoObject = ReadAsExpandoObject(reader, objectType, existingValue, serializer);
            return ParseAsListValue(expandoObject);
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
                WriteJson(writer, val.StructValue, serializer);
            }
            else if (val.Kind.Is(6))
            {
                WriteJson(writer, val.ListValue, serializer);
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

    private static Value ParseAsValue(object? value)
    {
        if (value is long and 0)
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

        if (value is IDictionary<string, object?> expandObject)
        {
            var fieldName = expandObject.Keys.FirstOrDefault();
            switch (fieldName)
            {
                case Struct.FieldName:
                    return new Value { StructValue = ParseAsStruct(expandObject) };

                case ListValue.FieldName:
                    return new Value { ListValue = ParseAsListValue(expandObject) };
            }
        }

        return new Value();
    }

    private static Struct ParseAsStruct(IDictionary<string, object?> expandoObject)
    {
        var @struct = new Struct();
        foreach (var field in (IDictionary<string, object?>)expandoObject[Struct.FieldName]!)
        {
            @struct.Fields.Add(field.Key, ParseAsValue(field.Value));
        }

        return @struct;
    }

    private static ListValue ParseAsListValue(IDictionary<string, object?> expandoObject)
    {
        var listValue = new ListValue();
        foreach (var item in (List<object>)expandoObject[ListValue.FieldName]!)
        {
            listValue.Values.Add(ParseAsValue(item));
        }

        return listValue;
    }
}