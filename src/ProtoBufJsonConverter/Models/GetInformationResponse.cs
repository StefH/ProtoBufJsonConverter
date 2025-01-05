namespace ProtoBufJsonConverter.Models;

public record GetInformationResponse
{
    public required string[] PackageNames { get; init; }

    public required string[] CSharpNamespaces { get; init; }

    public required IReadOnlyDictionary<string, Type> MessageTypes { get; init; }
}