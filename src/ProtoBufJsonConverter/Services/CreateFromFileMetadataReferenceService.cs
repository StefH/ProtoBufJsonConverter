using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Stef.Validation;

namespace ProtoBufJsonConverter.Services;

internal class CreateFromFileMetadataReferenceService : IMetadataReferenceService
{
    private readonly ConcurrentDictionary<string, MetadataReference> _cachedMetadataReferences = new();

    public Task<MetadataReference> CreateAsync(Assembly assembly, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(assembly);

        var key = assembly.GetName().FullName;

        return Task.FromResult(_cachedMetadataReferences.GetOrAdd(key, _ => MetadataReference.CreateFromFile(assembly.Location)));
    }
}