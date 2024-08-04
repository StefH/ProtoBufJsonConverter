using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.BytesValue", Origin = "google/protobuf/wrappers.proto")]
public struct BytesValue : IWellKnownType<byte[]>
{
    [ProtoMember(1)]
    public byte[] Value { get; set; }
}