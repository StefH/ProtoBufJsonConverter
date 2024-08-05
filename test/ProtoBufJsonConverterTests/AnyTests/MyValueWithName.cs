using ProtoBuf;

namespace ProtoBufJsonConverterTests.AnyTests;

public partial class AnyTests
{
    [ProtoContract(Name = ".custom.MyValueWithName")]
    public struct MyValueWithName
    {
        [ProtoMember(1)]
        public string Value { get; set; }
    }
}