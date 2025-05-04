using ProtoBuf;
using ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Interfaces;

namespace ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.StringValue", Origin = "google/protobuf/wrappers.proto")]
public struct Int64Value : IWellKnownType<long>
{
    [ProtoMember(1, IsRequired = true)]
    public long Value { get; set; }
}