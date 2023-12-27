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

    [Fact]
    public void ConvertToJsonRequest_WithGrpcHeader()
    {
        // Arrange
        var messageType = "greet.HelloRequest";

        var bytes = Convert.FromBase64String("AAAAAAYKBHN0ZWY=");
        
        var request = new ConvertToJsonRequest(ProtoDefinition, messageType, bytes);
        
        // Act
        var json = _sut.Convert(request);

        // Assert
        json.Should().Be(@"{""name"":""stef""}");
    }

    [Fact]
    public void ConvertToJsonRequest_WithoutGrpcHeader()
    {
        // Arrange
        var messageType = "greet.HelloRequest";

        var bytes = Convert.FromBase64String("CgRzdGVm");

        var request = new ConvertToJsonRequest(ProtoDefinition, messageType, bytes);

        // Act
        var json = _sut.Convert(request);

        // Assert
        json.Should().Be(@"{""name"":""stef""}");
    }
}