using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.Struct", Origin = "google/protobuf/struct.proto")]
public class Struct : IWellKnownType
{
    public const string FieldName = "fields";

    [ProtoMember(1, Name = FieldName)]
    [ProtoMap]
    public Dictionary<string, Value> Fields { get; } = new();
}