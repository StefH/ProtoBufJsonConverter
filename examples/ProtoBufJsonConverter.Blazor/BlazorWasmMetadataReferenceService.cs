using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using ProtoBufJsonConverter.Services;
using Stef.Validation;

namespace ProtoBufJsonConverter.Blazor;

/// <summary>
/// Based on https://github.com/LostBeard/BlazorWASMScriptLoader
/// </summary>
public class BlazorWasmMetadataReferenceService : IMetadataReferenceService
{
    private readonly HttpClient _httpClient = new();

    private readonly ConcurrentDictionary<string, MetadataReference> _cachedMetadataReferences = new();

    public BlazorWasmMetadataReferenceService(NavigationManager navigationManager)
    {
        _httpClient.BaseAddress = new Uri(Guard.NotNull(navigationManager).BaseUri);
    }

    public async Task<MetadataReference> CreateAsync(Assembly assembly, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(assembly);

        var assemblyName = assembly.GetName().Name!;
        if (_cachedMetadataReferences.TryGetValue(assemblyName, out var metadataReference))
        {
            return metadataReference;
        }

        var assemblyUrl = $"./_framework/{assemblyName}.dll";
        var httpResponseMessage = await _httpClient.GetAsync(assemblyUrl, cancellationToken);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var bytes = await httpResponseMessage.Content.ReadAsByteArrayAsync(cancellationToken);
            metadataReference = MetadataReference.CreateFromImage(bytes);

            _cachedMetadataReferences[assemblyName] = metadataReference;
            return metadataReference;
        }

        // https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly?view=aspnetcore-8.0#webcil-packaging-format-for-net-assemblies
        throw new Exception("ReferenceMetadata not found. If using .NET 8, <WasmEnableWebcil>false</WasmEnableWebcil> must be set in the project .csproj file.");
    }
}