using ProtoBuf;

namespace ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.DoubleValue", Origin = "google/protobuf/wrappers.proto")]
public struct DoubleValue : IWellKnownType<double>
{
    [ProtoMember(1)]
    public double Value { get; set; }
}