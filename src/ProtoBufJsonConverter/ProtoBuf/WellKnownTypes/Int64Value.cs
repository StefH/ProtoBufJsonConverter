using ProtoBuf;

namespace ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.StringValue", Origin = "google/protobuf/wrappers.proto")]
public struct Int64Value
{
    [ProtoMember(1)]
    public long Value { get; set; }
}