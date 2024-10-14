using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.ListValue", Origin = "google/protobuf/struct.proto")]
public class ListValue : IWellKnownType
{
    public const string FieldName = "values";

    [ProtoMember(1, Name = FieldName)]
    public List<Value> Values { get; } = new();
}