using System.Collections.Concurrent;
using MetadataReferenceService.Abstractions;
using MetadataReferenceService.Abstractions.Types;
using Microsoft.CodeAnalysis;
using Stef.Validation;

namespace ProtoBufJsonConverter.Services;

internal class CreateFromFileMetadataReferenceService : IMetadataReferenceService
{
    private readonly ConcurrentDictionary<string, MetadataReference> _cachedMetadataReferences = new();

    /// <inheritdoc />
    public Task<MetadataReference> CreateAsync(AssemblyDetails assembly, CancellationToken cancellationToken = default)
    {
        var key = Guard.NotNullOrWhiteSpace(assembly.Name);
        var location = Guard.NotNullOrWhiteSpace(assembly.Location);

        return Task.FromResult(_cachedMetadataReferences.GetOrAdd(key, _ => MetadataReference.CreateFromFile(location)));
    }
}