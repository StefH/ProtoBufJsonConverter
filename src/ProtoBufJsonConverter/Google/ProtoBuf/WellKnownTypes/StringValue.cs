using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.StringValue", Origin = "google/protobuf/wrappers.proto")]
public struct StringValue : IWellKnownType<string>
{
    [ProtoMember(1)]
    public string Value { get; set; }
}