using ProtoBuf;

namespace ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.UInt64Value", Origin = "google/protobuf/wrappers.proto")]
public struct UInt64Value
{
    [ProtoMember(1)]
    public ulong Value { get; set; }
}