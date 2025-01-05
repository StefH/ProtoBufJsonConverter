namespace ProtoBufJsonConverter.Models;

public record GetInformationResponse
{
    public required string[] PackageNames { get; init; }

    public required string[] CSharpNamespaces { get; init; }

    public required string[] MessageTypes { get; init; }
}