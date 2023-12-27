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
    public void GetMemoryStream(string data, bool skip, int expectedPosition, int expectedLength)
    {
        // Arrange
        var bytes = Convert.FromBase64String(data);

        var sut = new ProtoBufUtils(bytes);

        // Act
        var ms = sut.GetMemoryStream(skip);

        // Assert
        ms.Position.Should().Be(expectedPosition);
        ms.Length.Should().Be(expectedLength);
    }
}