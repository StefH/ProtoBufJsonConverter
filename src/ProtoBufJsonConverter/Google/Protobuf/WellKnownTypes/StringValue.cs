using ProtoBuf;
using ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Interfaces;

namespace ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.StringValue", Origin = "google/protobuf/wrappers.proto")]
public struct StringValue : IWellKnownType<string>
{
    [ProtoMember(1, IsRequired = true)]
    public string Value { get; set; }
}