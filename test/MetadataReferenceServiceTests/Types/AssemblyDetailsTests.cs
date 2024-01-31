using MetadataReferenceService.Abstractions.Types;

namespace MetadataReferenceServiceTests.Types;

public class AssemblyDetailsTests
{
    [Fact]
    public void AssemblyDetails_GetHashCode()
    {
        // Arrange
        var name = "test";
        var location = "abc";
        var image = "Stef"u8.ToArray();

        var assemblyDetails1 = new AssemblyDetails
        {
            Name = name,
            Location = location,
            Image = image
        };
        var assemblyDetails2 = new AssemblyDetails
        {
            Name = "te" + "st",
            Location = "ab" + "c",
            Image = image.ToArray()
        };

        // Act
        var hashCode1 = assemblyDetails1.GetHashCode();
        var hashCode2 = assemblyDetails2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }
}