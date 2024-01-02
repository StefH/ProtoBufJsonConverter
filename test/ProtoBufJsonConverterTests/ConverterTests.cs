using FluentAssertions;
using ProtoBufJsonConverter;
using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverterTests;

public class ConverterTests
{
    private const string ProtoDefinition = @"
syntax = ""proto3"";

package greet;

service Greeter
{
    rpc SayHello (HelloRequest) returns(HelloReply);
}

message HelloRequest
{
    string name = 1;
}

message HelloReply
{
    string message = 1;
}
";

    private readonly Converter _sut;

    public ConverterTests()
    {
        _sut = new Converter();
    }

    [Theory]
    [InlineData("AAAAAAYKBHN0ZWY=")]
    [InlineData("CgRzdGVm")]
    public async Task ConvertAsync_ConvertToJsonRequest_SkipGrpcHeader_IsTrue(string data)
    {
        // Arrange
        const string messageType = "greet.HelloRequest";

        var bytes = Convert.FromBase64String(data);

        var request = new ConvertToJsonRequest(ProtoDefinition, messageType, bytes);

        // Act
        var json = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        json.Should().Be(@"{""name"":""stef""}");
    }

    [Theory]
    [InlineData(true, "AAAAAAYKBHN0ZWY=")]
    [InlineData(false, "CgRzdGVm")]
    public async Task ConvertAsync_ConvertJsonToProtoBufRequest(bool addGrpcHeader, string expectedBytes)
    {
        // Arrange
        const string messageType = "greet.HelloRequest";

        const string json = @"{""name"":""stef""}";

        var request = new ConvertToProtoBufRequest(ProtoDefinition, messageType, json, addGrpcHeader);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be(expectedBytes);
    }

    [Theory]
    [InlineData(true, "AAAAAAYKBHN0ZWY=")]
    [InlineData(false, "CgRzdGVm")]
    public async Task ConvertAsync_ConvertObjectToProtoBufRequest(bool addGrpcHeader, string expectedBytes)
    {
        // Arrange
        const string messageType = "greet.HelloRequest";

        var @object = new { name = "stef" };

        var request = new ConvertToProtoBufRequest(ProtoDefinition, messageType, @object, addGrpcHeader);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be(expectedBytes);
    }
}