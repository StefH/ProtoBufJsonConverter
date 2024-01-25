using System.Collections.Concurrent;
using MetadataReferenceService.Abstractions;
using MetadataReferenceService.Abstractions.Types;
using Microsoft.CodeAnalysis;
using Stef.Validation;

namespace MetadataReferenceService.Default;

/// <summary>
/// Default implementation for <see cref="IMetadataReferenceService"/> which creates a <see cref="MetadataReference"/> from a Assembly-image;
/// </summary>
public class CreateFromImageMetadataReferenceService : IMetadataReferenceService
{
    private readonly ConcurrentDictionary<int, MetadataReference> _cachedMetadataReferences = new();

    /// <inheritdoc />
    public Task<MetadataReference> CreateAsync(AssemblyDetails assemblyDetails, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(assemblyDetails.Name);
        var image = Guard.NotNull(assemblyDetails.Image)!;

        var key = assemblyDetails.GetHashCode();
        return Task.FromResult(_cachedMetadataReferences.GetOrAdd(key, _ => MetadataReference.CreateFromImage(image)));
    }
}