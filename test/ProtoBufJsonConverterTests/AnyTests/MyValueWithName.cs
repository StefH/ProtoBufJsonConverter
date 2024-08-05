using ProtoBuf;

namespace ProtoBufJsonConverterTests.AnyTests;

[ProtoContract(Name = ".custom.MyValueWithName")]
public struct MyValueWithName
{
    [ProtoMember(1)]
    public string Value { get; set; }
}