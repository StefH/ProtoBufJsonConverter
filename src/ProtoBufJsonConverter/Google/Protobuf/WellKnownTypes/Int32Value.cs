using Google.Protobuf.WellKnownTypes.Interfaces;
using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.Int32Value", Origin = "google/protobuf/wrappers.proto")]
public struct Int32Value : IWellKnownType<int>
{
    [ProtoMember(1, IsRequired = true)]
    public int Value { get; set; }
}