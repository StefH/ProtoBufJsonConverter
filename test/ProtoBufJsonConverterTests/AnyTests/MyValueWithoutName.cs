using ProtoBuf;

namespace ProtoBufJsonConverterTests.AnyTests;

[ProtoContract]
public struct MyValueWithoutName
{
    [ProtoMember(1)]
    public string Value { get; set; }
}