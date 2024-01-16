using System.Reflection;
using Microsoft.CodeAnalysis;

namespace MetadataReferenceService.Abstractions;

public interface IMetadataReferenceService
{
    Task<MetadataReference> CreateAsync(Assembly assembly, CancellationToken cancellationToken = default);
}