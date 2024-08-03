using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

namespace ProtoBufJsonConverterTests;

public class AnyTests
{
    [Fact]
    public void PackAndUnpack()
    {
        // Arrange
        var stringValue = new StringValue { Value = "Test" };

        // Act 1
        var any = Any.Pack(stringValue);

        // Assert 1
        any.TypeUrl.Should().Be("type.googleapis.com/google.protobuf.StringValue");
        Convert.ToBase64String(any.Value.ToArray()).Should().Be("CgRUZXN0");

        // Act 2
        var unpackedStringValue = any.Unpack<StringValue>();

        // Assert 2
        unpackedStringValue.Value.Should().Be(stringValue.Value);
    }
}