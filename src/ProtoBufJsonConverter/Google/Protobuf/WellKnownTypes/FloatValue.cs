using ProtoBuf;
using ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Interfaces;

namespace ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.FloatValue", Origin = "google/protobuf/wrappers.proto")]
public struct FloatValue : IWellKnownType<float>
{
    [ProtoMember(1, IsRequired = true)]
    public float Value { get; set; }
}