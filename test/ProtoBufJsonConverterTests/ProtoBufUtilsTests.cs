using System.Buffers.Binary;
using System.Text;
using FluentAssertions;
using ProtoBufJsonConverter.Utils;

namespace ProtoBufJsonConverterTests;

public class ProtoBufUtilsTests
{
    [Theory]
    [InlineData("CgRzdGVm", true, 0, 6)]
    [InlineData("CgRzdGVm", false, 0, 6)]
    [InlineData("AAAAAAYKBHN0ZWY=", true, 5, 11)]
    [InlineData("AAAAAAYKBHN0ZWY=", false, 0, 11)]
    [InlineData("AAAAAAA=", false, 0, 5)]
    public void GetMemoryStreamFromBytes(string data, bool skip, int expectedPosition, int expectedLength)
    {
        // Arrange
        var bytes = Convert.FromBase64String(data);

        // Act
        var ms = ProtoBufUtils.GetMemoryStreamFromBytes(bytes, skip);

        // Assert
        ms.Position.Should().Be(expectedPosition);
        ms.Length.Should().Be(expectedLength);
    }

    [Fact]
    public void Serialize_WithGrpcHeader_AddsHeader()
    {
        // Arrange
        var data = new byte[] { 0x10, 0x20, 0x30 };

        // Act
        var result = ProtoBufUtils.Serialize(stream =>
        {
            stream.Write(data, 0, data.Length);
        }, addGrpcHeader: true);

        // Assert
        result.Should().ContainInOrder(0x00, 0x00, 0x00, 0x00, 0x03, 0x10, 0x20, 0x30);
    }

    [Fact]
    public void Serialize_WithoutGrpcHeader_DoesNotAddHeader()
    {
        // Arrange
        var data = "Hello World"u8.ToArray();

        // Act
        var result = ProtoBufUtils.Serialize(stream =>
        {
            stream.Write(data, 0, data.Length);
        }, addGrpcHeader: false);

        // Assert
        result.Length.Should().Be(data.Length);
        result.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void Serialize_WithEmptyDataAndWithoutGrpcHeader_ReturnsEmptyArray()
    {
        // Act
        var result = ProtoBufUtils.Serialize(_ => { }, addGrpcHeader: false);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Serialize_WithEmptyDataAndWithGrpcHeader_ReturnsEmptyHeader()
    {
        // Act
        var result = ProtoBufUtils.Serialize(_ => { }, addGrpcHeader: true);

        // Assert
        result.Should().ContainInOrder(0x00, 0x00, 0x00, 0x00, 0x00);
    }
}