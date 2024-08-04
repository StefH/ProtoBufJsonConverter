using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using ProtoBuf;

namespace ProtoBufJsonConverterTests;

public class AnyTests
{
    [ProtoContract(Name = ".google.protobuf.MyValueWithName")]
    public struct MyValueWithName
    {
        [ProtoMember(1)]
        public string Value { get; set; }
    }

    [ProtoContract]
    public struct MyValueWithoutName
    {
        [ProtoMember(1)]
        public string Value { get; set; }
    }

    [Fact]
    public void PackAndUnpack_StringValue()
    {
        // Arrange
        var value = new StringValue { Value = "Test" };

        // Act 1
        var any = Any.Pack(value);

        // Assert 1
        any.TypeUrl.Should().Be("type.googleapis.com/google.protobuf.StringValue");
        Convert.ToBase64String(any.Value.ToArray()).Should().Be("CgRUZXN0");

        // Act 2
        var unpackedStringValue = any.Unpack<StringValue>();

        // Assert 2
        unpackedStringValue.Value.Should().Be(value.Value);
    }

    [Fact]
    public void PackAndUnpack_MyValueWithName()
    {
        // Arrange
        var value = new MyValueWithName { Value = "Test" };

        // Act 1
        var any = Any.Pack(value);

        // Assert 1
        any.TypeUrl.Should().Be("custom/google.protobuf.MyValueWithName");
        Convert.ToBase64String(any.Value.ToArray()).Should().Be("CgRUZXN0");

        // Act 2
        var unpackedStringValue = any.Unpack<MyValueWithName>();

        // Assert 2
        unpackedStringValue.Value.Should().Be(value.Value);
    }

    [Fact]
    public void PackAndUnpack_MyValueWithoutName()
    {
        // Arrange
        var value = new MyValueWithoutName { Value = "Test" };

        // Act 1
        var any = Any.Pack(value);

        // Assert 1
        any.TypeUrl.Should().Be("custom/ProtoBufJsonConverterTests.AnyTests+MyValueWithoutName");
        Convert.ToBase64String(any.Value.ToArray()).Should().Be("CgRUZXN0");

        // Act 2
        var unpackedStringValue = any.Unpack<MyValueWithoutName>();

        // Assert 2
        unpackedStringValue.Value.Should().Be(value.Value);
    }
}