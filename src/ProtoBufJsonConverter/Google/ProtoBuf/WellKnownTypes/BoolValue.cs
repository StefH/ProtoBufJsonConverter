using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.BoolValue", Origin = "google/protobuf/wrappers.proto")]
public struct BoolValue : IWellKnownType<bool>
{
    [ProtoMember(1)]
    public bool Value { get; set; }
}