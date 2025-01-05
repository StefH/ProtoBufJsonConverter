using FluentAssertions;
using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverterTests;

public partial class ConverterTests
{
    [Fact]
    public async Task GetInformationAsync_WithCSharpNamespace_ReturnsExpectedInformation()
    {
        // Arrange
        var request = new GetInformationRequest(ProtoDefinitionWithCSharpNamespace);

        // Act
        var response = await _sut.GetInformationAsync(request).ConfigureAwait(false);

        // Assert
        response.PackageNames.Should().Equal("google.protobuf", "greet");
        response.CSharpNamespaces.Should().Equal("Google.Protobuf.WellKnownTypes", "Test");
        response.MessageTypes.Should().Equal("google.protobuf.Empty", "greet.HelloReply", "greet.HelloRequest", "greet.MyMessageEmpty");
    }

    [Fact]
    public async Task GetInformationAsync_WithWellKnownTypes_ReturnsExpectedInformation()
    {
        // Arrange
        var request = new GetInformationRequest(ProtoDefinitionWithWellKnownTypes);

        // Act
        var response = await _sut.GetInformationAsync(request).ConfigureAwait(false);

        // Assert
        response.PackageNames.Should().Equal("google.protobuf");
        response.CSharpNamespaces.Should().Equal("Google.Protobuf.WellKnownTypes");
        response.MessageTypes.Should().Equal("google.protobuf.Duration", "google.protobuf.Empty", "google.protobuf.Timestamp", "MyMessageDuration", "MyMessageEmpty", "MyMessageTimestamp");
    }

    [Fact]
    public async Task GetInformationAsync_ProtoDefinitionWithWellKnownTypesFromGoogle_ReturnsExpectedInformation()
    {
        // Arrange
        var request = new GetInformationRequest(ProtoDefinitionWithWellKnownTypesFromGoogle);

        // Act
        var response = await _sut.GetInformationAsync(request).ConfigureAwait(false);

        // Assert
        response.PackageNames.Should().Equal("google.protobuf");
        response.CSharpNamespaces.Should().Equal("Google.Protobuf.WellKnownTypes");

        response.MessageTypes.Should().Equal(
            "google.protobuf.Any",
            "google.protobuf.BoolValue", 
            "google.protobuf.BytesValue", 
            "google.protobuf.DoubleValue", 
            "google.protobuf.FloatValue", 
            "google.protobuf.Int32Value", 
            "google.protobuf.Int64Value", 
            "google.protobuf.ListValue",
            "google.protobuf.StringValue",
            "google.protobuf.Struct",
            "google.protobuf.UInt32Value",
            "google.protobuf.UInt64Value",
            "google.protobuf.Value",
            "MyMessageAny", 
            "MyMessageInt32Value", 
            "MyMessageInt64Value", 
            "MyMessageListValue", 
            "MyMessageNullValue", 
            "MyMessageStringValue", 
            "MyMessageStruct", 
            "MyMessageValue");
    }
}