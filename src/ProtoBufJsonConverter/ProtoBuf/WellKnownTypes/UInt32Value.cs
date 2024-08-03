using ProtoBuf;

namespace ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.UInt32Value", Origin = "google/protobuf/wrappers.proto")]
public struct UInt32Value
{
    [ProtoMember(1)]
    public uint Value { get; set; }
}