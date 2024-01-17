using System.Collections.Concurrent;
using System.Reflection;
using MetadataReferenceService.Abstractions;
using MetadataReferenceService.BlazorWasm.Models;
using MetadataReferenceService.BlazorWasm.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Stef.Validation;

namespace MetadataReferenceService.BlazorWasm;

/// <summary>
/// - https://github.com/LostBeard/BlazorWASMScriptLoader
/// - https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly?view=aspnetcore-8.0#webcil-packaging-format-for-net-assemblies
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

        var bytes = await DownloadFileAsync(assemblyName, cancellationToken);
        
        metadataReference = MetadataReference.CreateFromImage(bytes);
        
        _cachedMetadataReferences[assemblyName] = metadataReference;
        return metadataReference;
    }

    private async Task<byte[]> DownloadFileAsync(string assemblyName, CancellationToken cancellationToken)
    {
#if NET8_0
        var wasm = await TryDownloadFileAsync(assemblyName, FileType.Wasm, cancellationToken);
        if (wasm.Success)
        {
            return wasm.Bytes!;
        }

        var dll = await TryDownloadFileAsync(assemblyName, FileType.Dll, cancellationToken);
        if (dll.Success)
        {
            return dll.Bytes!;
        }
#else
        var dll = await TryDownloadFileAsync(assemblyName, FileType.Dll, cancellationToken);
        if (dll.Success)
        {
            return dll.Bytes!;
        }
#endif

        throw new FileNotFoundException("File not found.", assemblyName);
    }

    private async Task<DownloadFileResult> TryDownloadFileAsync(string assemblyName, FileType fileType, CancellationToken cancellationToken)
    {
        var assemblyUrl = $"./_framework/{assemblyName}.{fileType.ToString().ToLowerInvariant()}";
        var httpResponseMessage = await _httpClient.GetAsync(assemblyUrl, cancellationToken);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var bytes = await httpResponseMessage.Content.ReadAsByteArrayAsync(cancellationToken);
            return new(fileType, bytes);
        }

        return new(fileType);
    }
}