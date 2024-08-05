using ProtoBuf;

namespace ProtoBufJsonConverterTests.AnyTests;

public partial class AnyTests
{
    [ProtoContract]
    public struct MyValueWithoutName
    {
        [ProtoMember(1)]
        public string Value { get; set; }
    }
}