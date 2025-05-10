using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ProtoBuf.WellKnownTypes;
using ProtoBufJsonConverter.Extensions;
using ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes;
using ProtoBufJsonConverter.Utils;

namespace ProtoBufJsonConverter.Converters;

internal class WellKnownTypesConverter : JsonConverter
{
    private readonly ExpandoObjectConverter _converter = new();
    private readonly Func<Type, bool> _isDateTime = t => t == typeof(DateTime) || t == typeof(DateTime?);
    private readonly Func<Type, bool> _isTimestamp = t => t == typeof(Timestamp) || t == typeof(Timestamp?);
    private readonly Func<Type, bool> _isTimeSpan = t => t == typeof(TimeSpan) || t == typeof(TimeSpan?);
    private readonly Func<Type, bool> _isDuration = t => t == typeof(Duration) || t == typeof(Duration?);
    private readonly bool _supportNewerGoogleWellKnownTypes;
    private readonly IList<Func<Type, bool>> _supportedTypes;

    /// <summary>
    /// Constructor for WellKnownTypesConverter.
    /// </summary>
    /// <param name="supportNewerGoogleWellKnownTypes">
    /// Specify whether the well-known types 'google.protobuf.Timestamp' and 'google.protobuf.TimestampDuration' should be serialized to the newer definition.
    /// See also:
    /// - https://protobuf.dev/reference/protobuf/google.protobuf/#timestamp
    /// - https://protobuf.dev/reference/protobuf/google.protobuf/#duration
    /// </param>
    internal WellKnownTypesConverter(bool supportNewerGoogleWellKnownTypes)
    {
        _supportNewerGoogleWellKnownTypes = supportNewerGoogleWellKnownTypes;

        _supportedTypes = new List<Func<Type, bool>>
        {
            t => t.IsWellKnownType(),
            t => t == typeof(NullValue)
        };

        if (_supportNewerGoogleWellKnownTypes)
        {
            _supportedTypes.Add(_isDateTime);
            _supportedTypes.Add(_isTimestamp);
            _supportedTypes.Add(_isTimeSpan);
            _supportedTypes.Add(_isDuration);
        }
    }

    public override bool CanConvert(Type objectType)
    {
        return _supportedTypes.Any(f => f(objectType));
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (typeof(NullValue) == objectType)
        {
            return NullValue.NullValue;
        }

        if (typeof(Value) == objectType && TryReadAsExpandoObject(reader, objectType, existingValue, serializer, out var expandoObject))
        {
            return ParseAsValue(expandoObject);
        }

        if (typeof(Struct) == objectType && TryReadAsExpandoObject(reader, objectType, existingValue, serializer, out expandoObject))
        {
            return ParseAsStruct(expandoObject);
        }

        if (typeof(ListValue) == objectType && TryReadAsExpandoObject(reader, objectType, existingValue, serializer, out expandoObject))
        {
            return ParseAsListValue(expandoObject);
        }

        if (typeof(Any) == objectType && TryReadAsExpandoObject(reader, objectType, existingValue, serializer, out expandoObject))
        {
            var typeUrl = (string)expandoObject[Any.TypeUrlPropertyName]!;
            var value = expandoObject[Any.ValuePropertyName];
            var bytes = SerializeUtils.Serialize(value);

            return new Any
            {
                TypeUrl = typeUrl,
                Value = new ByteString(bytes)
            };
        }

        if (_supportNewerGoogleWellKnownTypes)
        {
            if (_isDateTime(objectType) && TryReadAsExpandoObject(reader, objectType, existingValue, serializer, out expandoObject))
            {
                return ParseAsDateTimeOrTimespan(expandoObject, (seconds, nanos) => new Timestamp(seconds, nanos).AsDateTime());
            }

            if (_isTimeSpan(objectType) && TryReadAsExpandoObject(reader, objectType, existingValue, serializer, out expandoObject))
            {
                return ParseAsDateTimeOrTimespan(expandoObject, (seconds, nanos) => new Duration(seconds, nanos).AsTimeSpan());
            }
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
            writer.WriteStartObject();

            if (val.Kind.Is(1))
            {
                writer.WritePropertyName(Value.FieldNameNullValue);
                writer.WriteValue(val.NullValue);
            }
            else if (val.Kind.Is(2))
            {
                writer.WritePropertyName(Value.FieldNameNumberValue);
                writer.WriteValue(val.NumberValue);
            }
            else if (val.Kind.Is(3))
            {
                writer.WritePropertyName(Value.FieldNameStringValue);
                writer.WriteValue(val.StringValue);
            }
            else if (val.Kind.Is(4))
            {
                writer.WritePropertyName(Value.FieldNameBoolValue);
                writer.WriteValue(val.BoolValue);
            }
            else if (val.Kind.Is(5))
            {
                writer.WritePropertyName(Value.FieldNameStructValue);
                WriteJson(writer, val.StructValue, serializer);
            }
            else if (val.Kind.Is(6))
            {
                writer.WritePropertyName(Value.FieldNameListValue);
                WriteJson(writer, val.ListValue, serializer);
            }

            writer.WriteEndObject();
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

        if (_supportNewerGoogleWellKnownTypes)
        {
            if (value is DateTime dateTime)
            {
                var timestampFromDateTime = new Timestamp(dateTime);
                WriteTimestampOrDuration(writer, timestampFromDateTime.Seconds, timestampFromDateTime.Nanoseconds);
                return;
            }

            if (value is Timestamp timestamp)
            {
                WriteTimestampOrDuration(writer, timestamp.Seconds, timestamp.Nanoseconds);
                return;
            }

            if (value is TimeSpan timespan)
            {
                var durationFromTimespan = new Duration(timespan);
                WriteTimestampOrDuration(writer, durationFromTimespan.Seconds, durationFromTimespan.Nanoseconds);
                return;
            }

            if (value is Duration duration)
            {
                WriteTimestampOrDuration(writer, duration.Seconds, duration.Nanoseconds);
            }
        }
    }

    /// <summary>
    /// The only way to find where this json object begins and ends is by reading it in as a generic ExpandoObject.
    /// Read an entire object from the reader.
    /// </summary>
    private bool TryReadAsExpandoObject(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer, [NotNullWhen(true)] out IDictionary<string, object?>? dictionary)
    {
        var value = _converter.ReadJson(reader, objectType, existingValue, serializer);

        if (value is IDictionary<string, object?> valueAsDictionary)
        {
            dictionary = new Dictionary<string, object?>(valueAsDictionary, StringComparer.OrdinalIgnoreCase);
            return true;
        }

        dictionary = null;
        return false;
    }

    private static void WriteTimestampOrDuration(JsonWriter writer, long seconds, int nanos)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("seconds");
        writer.WriteValue(seconds);
        writer.WritePropertyName("nanos");
        writer.WriteValue(nanos);
        writer.WriteEndObject();
    }

    private static Value ParseAsValue(IDictionary<string, object?> valueAsDictionary)
    {
        var propertyName = valueAsDictionary.Keys.FirstOrDefault();

        if (string.IsNullOrEmpty(propertyName))
        {
            return new Value();
        }

        if (propertyName == Value.FieldNameNullValue)
        {
            return new Value { NullValue = NullValue.NullValue };
        }

        var value = valueAsDictionary[propertyName];

        switch (propertyName)
        {
            case Value.FieldNameNumberValue:
                if (value is double numberValue)
                {
                    return new Value { NumberValue = numberValue };
                }
                break;

            case Value.FieldNameStringValue:
                if (value is string stringValue)
                {
                    return new Value { StringValue = stringValue };
                }
                break;

            case Value.FieldNameBoolValue:
                if (value is bool boolValue)
                {
                    return new Value { BoolValue = boolValue };
                }
                break;

            case Value.FieldNameStructValue:
                if (value is IDictionary<string, object?> structValue)
                {
                    return new Value { StructValue = ParseAsStruct(structValue) };
                }
                break;

            case Value.FieldNameListValue:
                if (value is IDictionary<string, object?> listValue)
                {
                    return new Value { ListValue = ParseAsListValue(listValue) };
                }
                break;
        }

        return new Value();
    }

    private static Struct ParseAsStruct(IDictionary<string, object?> expandoObject)
    {
        var @struct = new Struct();
        foreach (var field in (IDictionary<string, object?>)expandoObject[Struct.FieldName]!)
        {
            @struct.Fields.Add(field.Key, ParseAsValue((IDictionary<string, object?>)field.Value!));
        }

        return @struct;
    }

    private static ListValue ParseAsListValue(IDictionary<string, object?> expandoObject)
    {
        var listValue = new ListValue();
        foreach (var item in (List<object>)expandoObject[ListValue.FieldName]!)
        {
            listValue.Values.Add(ParseAsValue((IDictionary<string, object?>)item));
        }

        return listValue;
    }

    private static T ParseAsDateTimeOrTimespan<T>(IDictionary<string, object?> expandoObject, Func<long, int, T> func)
    {
        var seconds = TypeUtils.ChangeType(expandoObject["seconds"], (long)0);
        var nanos = TypeUtils.ChangeType(expandoObject["nanos"], 0);

        return func(seconds, nanos);
    }
}