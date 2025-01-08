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
        var response = await _sut.GetInformationAsync(request);

        // Assert
        response.PackageNames.Should().Equal("google.protobuf", "greet");
        response.CSharpNamespaces.Should().Equal("Google.Protobuf.WellKnownTypes", "Test");
        response.MessageTypes.Keys.Should().Equal("Google.Protobuf.WellKnownTypes.Empty", "Test.HelloReply", "Test.HelloRequest", "Test.MyMessageEmpty");
    }

    [Fact]
    public async Task GetInformationAsync_WithWellKnownTypes_ReturnsExpectedInformation()
    {
        // Arrange
        var request = new GetInformationRequest(ProtoDefinitionWithWellKnownTypes);

        // Act
        var response = await _sut.GetInformationAsync(request);

        // Assert
        response.PackageNames.Should().Equal("google.protobuf");
        response.CSharpNamespaces.Should().Equal("Google.Protobuf.WellKnownTypes");
        response.MessageTypes.Keys.Should().Equal("Google.Protobuf.WellKnownTypes.Duration", "Google.Protobuf.WellKnownTypes.Empty", "Google.Protobuf.WellKnownTypes.Timestamp", "MyMessageDuration", "MyMessageEmpty", "MyMessageTimestamp");
    }

    [Fact]
    public async Task GetInformationAsync_ProtoDefinitionWithWellKnownTypesFromGoogle_ReturnsExpectedInformation()
    {
        // Arrange
        var request = new GetInformationRequest(ProtoDefinitionWithWellKnownTypesFromGoogle);

        // Act
        var response = await _sut.GetInformationAsync(request);

        // Assert
        response.PackageNames.Should().Equal("google.protobuf");
        response.CSharpNamespaces.Should().Equal("Google.Protobuf.WellKnownTypes");
        response.MessageTypes.Keys.Should().Equal(
            "Google.Protobuf.WellKnownTypes.Any",
            "Google.Protobuf.WellKnownTypes.BoolValue",
            "Google.Protobuf.WellKnownTypes.BytesValue",
            "Google.Protobuf.WellKnownTypes.DoubleValue",
            "Google.Protobuf.WellKnownTypes.FloatValue",
            "Google.Protobuf.WellKnownTypes.Int32Value",
            "Google.Protobuf.WellKnownTypes.Int64Value",
            "Google.Protobuf.WellKnownTypes.ListValue",
            "Google.Protobuf.WellKnownTypes.StringValue",
            "Google.Protobuf.WellKnownTypes.Struct",
            "Google.Protobuf.WellKnownTypes.UInt32Value",
            "Google.Protobuf.WellKnownTypes.UInt64Value",
            "Google.Protobuf.WellKnownTypes.Value",
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