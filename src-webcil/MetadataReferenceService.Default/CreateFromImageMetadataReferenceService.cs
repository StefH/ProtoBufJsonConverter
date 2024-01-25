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
    public Task<MetadataReference> CreateAsync(AssemblyDetails assembly, CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrWhiteSpace(assembly.Name);
        var image = Guard.NotNull(assembly.Image)!;

        var key = assembly.GetHashCode();
        return Task.FromResult(_cachedMetadataReferences.GetOrAdd(key, _ => MetadataReference.CreateFromImage(image)));
    }
}