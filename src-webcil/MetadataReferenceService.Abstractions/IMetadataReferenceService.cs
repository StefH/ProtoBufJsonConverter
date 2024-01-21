using System.Reflection;
using MetadataReferenceService.Abstractions.Types;
using Microsoft.CodeAnalysis;

namespace MetadataReferenceService.Abstractions;

/// <summary>
/// Interface representing the Metadata Reference Service.
/// </summary>
public interface IMetadataReferenceService
{
    /// <summary>
    /// Asynchronously creates a <see cref="MetadataReference"/> from a given <see cref="Assembly"/>.
    /// </summary>
    /// <param name="assembly">The AssemblyDetails which is used to create the MetadataReference.</param>
    /// <param name="cancellationToken">Optional. Used to signal cancellation of the operation.</param>
    /// <returns>A Task that, when completed, yields a <see cref="MetadataReference"/> object.</returns>
    Task<MetadataReference> CreateAsync(AssemblyDetails assembly, CancellationToken cancellationToken = default);
}