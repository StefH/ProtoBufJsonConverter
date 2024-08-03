using ProtoBuf;

namespace ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.BytesValue", Origin = "google/protobuf/wrappers.proto")]
public struct BytesValue
{
    [ProtoMember(1)]
    public ByteString Value { get; set; }
}