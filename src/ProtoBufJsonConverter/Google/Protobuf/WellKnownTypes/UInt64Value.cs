using ProtoBuf;
using ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Interfaces;

namespace ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.UInt64Value", Origin = "google/protobuf/wrappers.proto")]
public struct UInt64Value : IWellKnownType<ulong>
{
    [ProtoMember(1, IsRequired = true)]
    public ulong Value { get; set; }
}