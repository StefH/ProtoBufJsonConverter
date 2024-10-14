using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.ListValue", Origin = "google/protobuf/struct.proto")]
public class ListValue : IWellKnownType
{
    [ProtoMember(1, Name = "values")]
    public List<Value> Values { get; } = new();
}