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
        response.MessageTypes.Should().Equal("Empty", "HelloReply", "HelloRequest", "MyMessageEmpty");
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
        response.MessageTypes.Should().Equal("Duration", "Empty", "MyMessageDuration", "MyMessageEmpty", "MyMessageTimestamp", "Timestamp");
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
            "Any", 
            "BoolValue", 
            "BytesValue", 
            "DoubleValue", 
            "FloatValue", 
            "Int32Value", 
            "Int64Value", 
            "ListValue", 
            "MyMessageAny", 
            "MyMessageInt32Value", 
            "MyMessageInt64Value", 
            "MyMessageListValue", 
            "MyMessageNullValue", 
            "MyMessageStringValue", 
            "MyMessageStruct", 
            "MyMessageValue", 
            "StringValue", 
            "Struct", 
            "UInt32Value", 
            "UInt64Value", 
            "Value");
    }
}