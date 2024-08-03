using ProtoBuf;

namespace ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.BoolValue", Origin = "google/protobuf/wrappers.proto")]
public struct BoolValue
{
    [ProtoMember(1)]
    public bool Value { get; set; }
}