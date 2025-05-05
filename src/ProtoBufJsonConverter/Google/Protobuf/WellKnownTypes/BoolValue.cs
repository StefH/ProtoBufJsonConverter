using ProtoBuf;
using ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Interfaces;

namespace ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.BoolValue", Origin = "google/protobuf/wrappers.proto")]
public struct BoolValue : IWellKnownType<bool>
{
    [ProtoMember(1, IsRequired = true)]
    public bool Value { get; set; }
}