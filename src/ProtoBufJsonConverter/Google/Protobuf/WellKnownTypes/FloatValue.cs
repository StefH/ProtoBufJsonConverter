using Google.Protobuf.WellKnownTypes.Interfaces;
using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.FloatValue", Origin = "google/protobuf/wrappers.proto")]
public struct FloatValue : IWellKnownType<float>
{
    [ProtoMember(1, IsRequired = true)]
    public float Value { get; set; }
}