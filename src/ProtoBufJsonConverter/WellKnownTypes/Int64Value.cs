// ReSharper disable InconsistentNaming
using ProtoBuf;

namespace ProtoBufJsonConverter.WellKnownTypes;

[ProtoContract]
public class Int64Value
{
    [ProtoMember(1)]
    public long Value { get; set; }
}