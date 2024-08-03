// ReSharper disable InconsistentNaming
using ProtoBuf;

namespace ProtoBufJsonConverter.WellKnownTypes;

[ProtoContract]
public class StringValue
{
    [ProtoMember(1)]
    public string Value { get; set; } = string.Empty;
}