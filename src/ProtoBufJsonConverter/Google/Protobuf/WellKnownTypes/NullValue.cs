using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.NullValue", Origin = "google/protobuf/struct.proto")]
public enum NullValue
{
    [ProtoEnum(Name = "NULL_VALUE")]
    NullValue = 0,
}