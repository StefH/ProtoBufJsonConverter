using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.Duration", Origin = "google/protobuf/duration.proto")]
public struct DurationValue : IWellKnownType
{
    public const string FieldNameSeconds = "seconds";
    public const string FieldNameNanos = "nanos";

    [ProtoMember(1, Name = FieldNameSeconds, IsRequired = true)]
    public long Seconds { get; set; }

    [ProtoMember(2, Name = FieldNameNanos, IsRequired = true)]
    public int Nanos { get; set; }
}