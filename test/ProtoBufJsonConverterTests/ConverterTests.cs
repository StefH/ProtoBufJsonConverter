using FluentAssertions;
using ProtoBufJsonConverter;
using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverterTests;

public class ConverterTests
{
    private const string ProtoDefinitionNoPackage = @"
syntax = ""proto3"";

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

    private const string ProtoDefinition = @"
syntax = ""proto3"";

import ""google/protobuf/empty.proto"";

package greet;

service Greeter
{
    rpc SayHello (HelloRequest) returns(HelloReply);

    rpc SayNothing (google.protobuf.Empty) returns (google.protobuf.Empty);
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

    private const string ProtoDefinitionWithDotInPackage = @"
syntax = ""proto3"";

package greet.foo;

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

    private const string ProtoDefinitionWithDotsInPackage = @"
syntax = ""proto3"";

package greet.foo.bar;

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

    private const string ProtoDefinitionWithWellKnownTypes = @"
syntax = ""proto3"";

import ""google/protobuf/empty.proto"";
import ""google/protobuf/timestamp.proto"";
import ""google/protobuf/duration.proto"";

service Greeter
{
    rpc SayNothing (google.protobuf.Empty) returns (google.protobuf.Empty);
}

message MyMessageTimestamp
{
    google.protobuf.Timestamp ts = 1;
}

message MyMessageDuration
{
    google.protobuf.Duration du = 1;
}
";

    private const string ProtoDefinitionWithWellKnownTypesFromGoogle = @"
syntax = ""proto3"";

import ""google/protobuf/wrappers.proto"";

message MyMessage
{
    google.protobuf.StringValue str = 1;
}
";

    private readonly Converter _sut = new();

    [Theory]
    [InlineData("AAAAAAYKBHN0ZWY=", ProtoDefinitionNoPackage, "HelloRequest")]
    [InlineData("CgRzdGVm", ProtoDefinitionNoPackage, "HelloRequest")]
    [InlineData("AAAAAAYKBHN0ZWY=", ProtoDefinition, "greet.HelloRequest")]
    [InlineData("CgRzdGVm", ProtoDefinition, "greet.HelloRequest")]
    [InlineData("AAAAAAYKBHN0ZWY=", ProtoDefinitionWithDotInPackage, "greet.foo.HelloRequest")]
    [InlineData("CgRzdGVm", ProtoDefinitionWithDotInPackage, "greet.foo.HelloRequest")]
    [InlineData("AAAAAAYKBHN0ZWY=", ProtoDefinitionWithDotsInPackage, "greet.foo.bar.HelloRequest")]
    [InlineData("CgRzdGVm", ProtoDefinitionWithDotsInPackage, "greet.foo.bar.HelloRequest")]
    public async Task ConvertAsync_ConvertToJsonRequest_SkipGrpcHeader_IsTrue(string data, string protoDefinition, string messageType)
    {
        // Arrange
        var bytes = Convert.FromBase64String(data);

        var request = new ConvertToJsonRequest(protoDefinition, messageType, bytes);

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

    [Fact]
    public async Task ConvertAsync_WellKnownTypesEmpty_ConvertJsonToProtoBufRequest()
    {
        // Arrange
        const string messageType = "google.protobuf.Empty";

        const string json = "{}";

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, json);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        bytes.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypesEmpty_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "google.protobuf.Empty";

        var @object = new { };

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, @object);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        bytes.Should().BeEmpty();
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypesTimeStamp_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageTimestamp";

        var datetime = new DateTime(2024, 7, 30, 1, 2, 3);
        var @object = new { ts = datetime };

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, @object);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be("CgYIi/egtQY=");
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypesDuration_ConvertObjectToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessageDuration";

        var timespan = new TimeSpan(1, 2, 3, 4, 5, 6);
        var @object = new { du = timespan };

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypes, messageType, @object);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be("CgkI2NwFELDFsQI=");
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypesFromGoogleStringValue_ConvertJsonToProtoBufRequest()
    {
        // Arrange
        const string messageType = "google.protobuf.StringValue";
        const string json1 = @"{ ""value"": ""stef"" }";
        var convertToProtoBufRequest = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, json1);

        // Act 1
        var bytes = await _sut.ConvertAsync(convertToProtoBufRequest).ConfigureAwait(false);

        // Assert 1
        Convert.ToBase64String(bytes).Should().Be("CgRzdGVm");

        // Act 2
        var convertToJsonRequest = new ConvertToJsonRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, bytes);
        var json = await _sut.ConvertAsync(convertToJsonRequest).ConfigureAwait(false);

        // Assert 2
        json.Should().Be(@"{""Value"":""stef""}");
    }

    [Fact]
    public async Task ConvertAsync_WellKnownTypesFromGoogle_ConvertJsonToProtoBufRequest()
    {
        // Arrange
        const string messageType = "MyMessage";

        const string json = @"{ ""str"": { ""value"": ""stef"" } }";

        var request = new ConvertToProtoBufRequest(ProtoDefinitionWithWellKnownTypesFromGoogle, messageType, json);

        // Act
        var bytes = await _sut.ConvertAsync(request).ConfigureAwait(false);

        // Assert
        Convert.ToBase64String(bytes).Should().Be("CgkI2NwFELDFsQI=");
    }
}