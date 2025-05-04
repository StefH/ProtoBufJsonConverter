using System.ComponentModel;
using ProtoBuf;
using ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Interfaces;

namespace ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.Value", Origin = "google/protobuf/struct.proto")]
public class Value : IWellKnownType
{
    public const string FieldNameNullValue = "null_value";
    public const string FieldNameNumberValue = "number_value";
    public const string FieldNameStringValue = "string_value";
    public const string FieldNameBoolValue = "bool_value";
    public const string FieldNameStructValue = "struct_value";
    public const string FieldNameListValue = "list_value";

    private DiscriminatedUnion64Object _kind;

    public Value()
    {
    }

    public Value(NullValue value)
    {
        NullValue = value;
    }

    public Value(double value)
    {
        NumberValue = value;
    }

    public Value(string value)
    {
        StringValue = value;
    }

    public Value(bool value)
    {
        BoolValue = value;
    }

    public Value(Struct value)
    {
        StructValue = value;
    }

    public Value(ListValue value)
    {
        ListValue = value;
    }

    public DiscriminatedUnion64Object Kind => _kind;

    [ProtoMember(1, Name = FieldNameNullValue)]
    public NullValue NullValue
    {
        get => _kind.Is(1) ? (NullValue)_kind.Int32 : default;
        set => _kind = new DiscriminatedUnion64Object(1, (int)value);
    }
    public bool ShouldSerializeNullValue() => _kind.Is(1);
    public void ResetNullValue() => DiscriminatedUnion64Object.Reset(ref _kind, 1);

    [ProtoMember(2, Name = FieldNameNumberValue)]
    public double NumberValue
    {
        get => _kind.Is(2) ? _kind.Double : 0;
        set => _kind = new DiscriminatedUnion64Object(2, value);
    }
    public bool ShouldSerializeNumberValue() => _kind.Is(2);
    public void ResetNumberValue() => DiscriminatedUnion64Object.Reset(ref _kind, 2);

    [ProtoMember(3, Name = FieldNameStringValue)]
    [DefaultValue("")]
    public string StringValue
    {
        get => _kind.Is(3) ? (string)_kind.Object : string.Empty;
        set => _kind = new DiscriminatedUnion64Object(3, value);
    }
    public bool ShouldSerializeStringValue() => _kind.Is(3);
    public void ResetStringValue() => DiscriminatedUnion64Object.Reset(ref _kind, 3);

    [ProtoMember(4, Name = FieldNameBoolValue)]
    public bool BoolValue
    {
        get => _kind.Is(4) && _kind.Boolean;
        set => _kind = new DiscriminatedUnion64Object(4, value);
    }
    public bool ShouldSerializeBoolValue() => _kind.Is(4);
    public void ResetBoolValue() => DiscriminatedUnion64Object.Reset(ref _kind, 4);

    [ProtoMember(5, Name = FieldNameStructValue)]
    public Struct StructValue
    {
        get => _kind.Is(5) ? (Struct)_kind.Object : new Struct();
        set => _kind = new DiscriminatedUnion64Object(5, value);
    }
    public bool ShouldSerializeStructValue() => _kind.Is(5);
    public void ResetStructValue() => DiscriminatedUnion64Object.Reset(ref _kind, 5);

    [ProtoMember(6, Name = FieldNameListValue)]
    public ListValue ListValue
    {
        get => _kind.Is(6) ? (ListValue)_kind.Object : new ListValue();
        set => _kind = new DiscriminatedUnion64Object(6, value);
    }
    public bool ShouldSerializeListValue() => _kind.Is(6);
    public void ResetListValue() => DiscriminatedUnion64Object.Reset(ref _kind, 6);
}