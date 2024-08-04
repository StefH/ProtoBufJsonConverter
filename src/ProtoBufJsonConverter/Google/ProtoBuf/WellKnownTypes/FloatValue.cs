using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.FloatValue", Origin = "google/protobuf/wrappers.proto")]
public struct FloatValue : IWellKnownType<float>
{
    [ProtoMember(1)]
    public float Value { get; set; }
}