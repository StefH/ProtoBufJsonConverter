using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.UInt64Value", Origin = "google/protobuf/wrappers.proto")]
public struct UInt64Value : IWellKnownType<ulong>
{
    [ProtoMember(1)]
    public ulong Value { get; set; }
}