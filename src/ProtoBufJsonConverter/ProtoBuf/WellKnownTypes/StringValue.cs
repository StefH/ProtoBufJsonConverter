using ProtoBuf;

namespace ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.StringValue", Origin = "google/protobuf/wrappers.proto")]
public struct StringValue : IWellKnownType<string>
{
    [ProtoMember(1)]
    public string Value { get; set; }
}