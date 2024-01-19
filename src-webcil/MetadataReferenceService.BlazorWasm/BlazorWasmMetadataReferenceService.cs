using System.Collections.Concurrent;
using System.Reflection;
using MetadataReferenceService.Abstractions;
using MetadataReferenceService.BlazorWasm.Models;
using MetadataReferenceService.BlazorWasm.Types;
using MetadataReferenceService.BlazorWasm.WasmWebcil;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Stef.Validation;

namespace MetadataReferenceService.BlazorWasm;

/// <summary>
/// A Blazor Wasm Implementation of <see cref="IMetadataReferenceService"/>.
/// - https://github.com/LostBeard/BlazorWASMScriptLoader
/// - https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly?view=aspnetcore-8.0#webcil-packaging-format-for-net-assemblies
/// </summary>
public class BlazorWasmMetadataReferenceService : IMetadataReferenceService
{
    private readonly HttpClient _httpClient = new();

    private readonly ConcurrentDictionary<string, MetadataReference> _cachedMetadataReferences = new();

    /// <summary>
    /// Initializes a new instance of the BlazorWasmMetadataReferenceService class.
    /// </summary>
    /// <remarks>
    /// This constructor sets the base address of the internal HTTP client
    /// to the base URI of the application, as provided by the NavigationManager.
    /// </remarks>
    /// <param name="navigationManager">
    /// An instance of NavigationManager used to obtain the application's base URI.
    /// The navigation manager is validated to ensure it's not null before use.
    /// </param>
    public BlazorWasmMetadataReferenceService(NavigationManager navigationManager)
    {
        _httpClient.BaseAddress = new Uri(Guard.NotNull(navigationManager).BaseUri);
    }

    /// <inheritdoc />
    public async Task<MetadataReference> CreateAsync(Assembly assembly, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(assembly);

        var assemblyName = assembly.GetName().Name!;
        if (_cachedMetadataReferences.TryGetValue(assemblyName, out var metadataReference))
        {
            return metadataReference;
        }

        var downloadFileResult = await DownloadFileAsync(assemblyName, cancellationToken);
        if (downloadFileResult.Success)
        {
            if (downloadFileResult.FileType == FileType.Dll)
            {
                metadataReference = MetadataReference.CreateFromStream(downloadFileResult.Stream!);
            }
            else
            {
                var dllBytes = WebcilConverterUtil.ConvertFromWebcil(downloadFileResult.Stream!);
                metadataReference = MetadataReference.CreateFromImage(dllBytes);
            }

            _cachedMetadataReferences[assemblyName] = metadataReference;
            return metadataReference;
        }

        throw new InvalidOperationException($"Unable to download '{assemblyName}' and create a {nameof(MetadataReference)}.");
    }

    private async Task<DownloadFileResult> DownloadFileAsync(string assemblyName, CancellationToken cancellationToken)
    {
#if NET8_0
        // For .NET 8: First try to download the .wasm file ("<WasmEnableWebcil>true</WasmEnableWebcil>" --> default value)
        var wasm = await TryDownloadFileAsync(assemblyName, FileType.Wasm, cancellationToken);
        if (wasm.Success)
        {
            return wasm;
        }

        // For .NET 8: In case "<WasmEnableWebcil>false</WasmEnableWebcil>", download the .dll file
        var dll = await TryDownloadFileAsync(assemblyName, FileType.Dll, cancellationToken);
        if (dll.Success)
        {
            return dll;
        }
#else
        // For .NET 7 or lower, download the .dll file
        var dll = await TryDownloadFileAsync(assemblyName, FileType.Dll, cancellationToken);
        if (dll.Success)
        {
            return dll;
        }
#endif

        throw new FileNotFoundException("File not found.", BuildFilename(assemblyName));
    }

    private async Task<DownloadFileResult> TryDownloadFileAsync(string assemblyName, FileType fileType, CancellationToken cancellationToken)
    {
        var assemblyDownloadUrl = $"./_framework/{BuildFilename(assemblyName, fileType)}";
        var httpResponseMessage = await _httpClient.GetAsync(assemblyDownloadUrl, cancellationToken);
        if (httpResponseMessage.IsSuccessStatusCode)
        {
            var bytes = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);
            return new(fileType, bytes);
        }

        return new(fileType);
    }

    private static string BuildFilename(string assemblyName, FileType fileType = FileType.Both)
    {
        return fileType != FileType.Both ?
            $"{assemblyName}.{fileType.ToString().ToLowerInvariant()}" :
            $"{assemblyName}.{FileType.Dll.ToString().ToLowerInvariant()} or {assemblyName}.{FileType.Wasm.ToString().ToLowerInvariant()}";
    }
}