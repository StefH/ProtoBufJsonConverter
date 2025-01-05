using Google.Protobuf.WellKnownTypes.Interfaces;
using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.DoubleValue", Origin = "google/protobuf/wrappers.proto")]
public struct DoubleValue : IWellKnownType<double>
{
    [ProtoMember(1, IsRequired = true)]
    public double Value { get; set; }
}