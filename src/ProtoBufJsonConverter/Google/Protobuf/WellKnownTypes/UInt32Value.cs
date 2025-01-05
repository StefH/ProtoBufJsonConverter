using Google.Protobuf.WellKnownTypes.Interfaces;
using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.UInt32Value", Origin = "google/protobuf/wrappers.proto")]
public struct UInt32Value : IWellKnownType<uint>
{
    [ProtoMember(1, IsRequired = true)]
    public uint Value { get; set; }
}