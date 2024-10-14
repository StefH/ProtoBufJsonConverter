using System.ComponentModel;
using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.Value", Origin = "google/protobuf/struct.proto")]
public class Value : IWellKnownType
{
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

    [ProtoMember(1, Name = "null_value")]
    public NullValue NullValue
    {
        get => _kind.Is(1) ? (NullValue)_kind.Int32 : default;
        set => _kind = new DiscriminatedUnion64Object(1, (int)value);
    }
    public bool ShouldSerializeNullValue() => _kind.Is(1);
    public void ResetNullValue() => DiscriminatedUnion64Object.Reset(ref _kind, 1);

    [ProtoMember(2, Name = "number_value")]
    public double NumberValue
    {
        get => _kind.Is(2) ? _kind.Double : default;
        set => _kind = new DiscriminatedUnion64Object(2, value);
    }
    public bool ShouldSerializeNumberValue() => _kind.Is(2);
    public void ResetNumberValue() => DiscriminatedUnion64Object.Reset(ref _kind, 2);

    [ProtoMember(3, Name = "string_value")]
    [DefaultValue("")]
    public string StringValue
    {
        get => _kind.Is(3) ? (string)_kind.Object : string.Empty;
        set => _kind = new DiscriminatedUnion64Object(3, value);
    }
    public bool ShouldSerializeStringValue() => _kind.Is(3);
    public void ResetStringValue() => DiscriminatedUnion64Object.Reset(ref _kind, 3);

    [ProtoMember(4, Name = "bool_value")]
    public bool BoolValue
    {
        get => _kind.Is(4) ? _kind.Boolean : default;
        set => _kind = new DiscriminatedUnion64Object(4, value);
    }
    public bool ShouldSerializeBoolValue() => _kind.Is(4);
    public void ResetBoolValue() => DiscriminatedUnion64Object.Reset(ref _kind, 4);

    [ProtoMember(5, Name = "struct_value")]
    public Struct StructValue
    {
        get => _kind.Is(5) ? (Struct)_kind.Object : new Struct();
        set => _kind = new DiscriminatedUnion64Object(5, value);
    }
    public bool ShouldSerializeStructValue() => _kind.Is(5);
    public void ResetStructValue() => DiscriminatedUnion64Object.Reset(ref _kind, 5);

    [ProtoMember(6, Name = "list_value")]
    public ListValue ListValue
    {
        get => _kind.Is(6) ? (ListValue)_kind.Object : new ListValue();
        set => _kind = new DiscriminatedUnion64Object(6, value);
    }
    public bool ShouldSerializeListValue() => _kind.Is(6);
    public void ResetListValue() => DiscriminatedUnion64Object.Reset(ref _kind, 6);
}