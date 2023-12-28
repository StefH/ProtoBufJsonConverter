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
    public void ConvertToJsonRequest_SkipGrpcHeader_IsTrue(string data)
    {
        // Arrange
        const string messageType = "greet.HelloRequest";

        var bytes = Convert.FromBase64String(data);

        var request = new ConvertToJsonRequest(ProtoDefinition, messageType, bytes);

        // Act
        var json = _sut.Convert(request);

        // Assert
        json.Should().Be(@"{""name"":""stef""}");
    }

    [Theory]
    [InlineData(true, "AAAAAAYKBHN0ZWY=")]
    [InlineData(false, "CgRzdGVm")]
    public void ConvertToProtoBufRequest(bool addGrpcHeader, string expectedBytes)
    {
        // Arrange
        const string messageType = "greet.HelloRequest";

        const string json = @"{""name"":""stef""}";

        var request = new ConvertToProtoBufRequest(ProtoDefinition, messageType, json, addGrpcHeader);

        // Act
        var bytes = _sut.Convert(request);

        // Assert
        Convert.ToBase64String(bytes).Should().Be(expectedBytes);
    }
}