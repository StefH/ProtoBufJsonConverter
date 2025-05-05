using FluentAssertions;
using ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes;

namespace ProtoBufJsonConverterTests.AnyTests;

public class AnyTests
{
    [Fact]
    public void PackAndUnpack_StringValue()
    {
        // Arrange
        var test = "Test";
        var value = new StringValue { Value = test };

        // Act 1
        var any = Any.Pack(value);

        // Assert 1
        any.TypeUrl.Should().Be("type.googleapis.com/google.protobuf.StringValue");
        Convert.ToBase64String(any.Value.ToArray()).Should().Be("CgRUZXN0");

        // Act 2
        var unpackedStringValue = any.Unpack<StringValue>();

        // Assert 2
        unpackedStringValue.Value.Should().Be(test);

        // Act 3
        var unwrapped = any.GetUnwrappedValue();

        // Assert 3
        unwrapped.Should().Be(test);
    }

    [Fact]
    public void PackAndUnpack_FloatValue()
    {
        // Arrange
        var test = 123.456f;
        var value = new FloatValue { Value = test };

        // Act 1
        var any = Any.Pack(value);

        // Assert 1
        any.TypeUrl.Should().Be("type.googleapis.com/google.protobuf.FloatValue");
        Convert.ToBase64String(any.Value.ToArray()).Should().Be("DXnp9kI=");

        // Act 2
        var unpackedValue = any.Unpack<FloatValue>();

        // Assert 2
        unpackedValue.Value.Should().Be(test);

        // Act 3
        var unwrapped = any.GetUnwrappedValue();

        // Assert 3
        unwrapped.Should().Be(test);
    }

    [Fact]
    public void PackAndUnpack_MyValueWithName()
    {
        // Arrange
        var test = "Test";
        var value = new MyValueWithName { Value = test };

        // Act 1
        var any = Any.Pack(value);

        // Assert 1
        any.TypeUrl.Should().Be("type.googleapis.com/custom.MyValueWithName");
        Convert.ToBase64String(any.Value.ToArray()).Should().Be("CgRUZXN0");

        // Act 2
        var unpackedValue = any.Unpack<MyValueWithName>();

        // Assert 2
        unpackedValue.Value.Should().Be(test);

        // Act 3
        var result = any.TryGetUnwrappedValue<MyValueWithName>(out var unwrapped);

        // Assert 3
        result.Should().BeTrue();
        unwrapped.Value.Should().Be(test);
    }

    [Fact]
    public void PackAndUnpack_MyValueWithoutName()
    {
        // Arrange
        var test = "Test";
        var value = new MyValueWithoutName { Value = test };

        // Act 1
        var any = Any.Pack(value);

        // Assert 1
        any.TypeUrl.Should().Be("type.googleapis.com/ProtoBufJsonConverterTests.AnyTests.MyValueWithoutName");
        Convert.ToBase64String(any.Value.ToArray()).Should().Be("CgRUZXN0");

        // Act 2
        var unpackedValue = any.Unpack<MyValueWithoutName>();

        // Assert 2
        unpackedValue.Value.Should().Be(value.Value);

        // Act 3
        var result = any.TryGetUnwrappedValue<MyValueWithoutName>(out var unwrapped);

        // Assert 3
        result.Should().BeTrue();
        unwrapped.Value.Should().Be(test);
    }

    [Fact]
    public void PackAndUnpack_HelloRequestWithoutNamespace()
    {
        // Arrange
        var helloRequest = new HelloRequest { name = "Test" };

        // Act 1
        var any = Any.Pack(helloRequest);

        // Assert 1
        any.TypeUrl.Should().Be("type.googleapis.com/HelloRequest");
        Convert.ToBase64String(any.Value.ToArray()).Should().Be("CgRUZXN0");

        // Act 2
        var unpackedHelloRequest = any.Unpack<HelloRequest>();

        // Assert 2
        unpackedHelloRequest.name.Should().Be(helloRequest.name);

        // Act 3
        var result = any.TryGetUnwrappedValue<HelloRequest>(out var unwrapped);

        // Assert 3
        result.Should().BeTrue();
        unwrapped.Should().BeEquivalentTo(helloRequest);
    }

    [Fact]
    public void PackAndUnpack_HelloRequestWithNamespace()
    {
        // Arrange
        var helloRequest = new greet.HelloRequest { name = "Test" };

        // Act 1
        var any = Any.Pack(helloRequest);

        // Assert 1
        any.TypeUrl.Should().Be("type.googleapis.com/greet.HelloRequest");
        Convert.ToBase64String(any.Value.ToArray()).Should().Be("CgRUZXN0");

        // Act 2
        var unpackedHelloRequest = any.Unpack<greet.HelloRequest>();

        // Assert 2
        unpackedHelloRequest.name.Should().Be(helloRequest.name);

        // Act 3
        var result = any.TryGetUnwrappedValue<greet.HelloRequest>(out var unwrapped);

        // Assert 3
        result.Should().BeTrue();
        unwrapped.Should().BeEquivalentTo(helloRequest);
    }
}