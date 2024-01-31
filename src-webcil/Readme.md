# Webcil

## Info
Generate a valid MetadataReference based on a Wasm wrapped Webcil stream (`MetadataReferenceService.BlazorWasm`)

This project uses a slightly modified version from the source code for [Microsoft.NET.WebAssembly.Webcil](https://github.com/dotnet/runtime/blob/main/src/tasks/Microsoft.NET.WebAssembly.Webcil) in addition to some own code to unwrap a Wasm Webcil stream into a Portable Executable (byte[]) which can be used to create a MetadataReferenceService.
The reason for this is that by default the `WasmEnableWebcil` is set to `true`:
``` xml
<WasmEnableWebcil>true</WasmEnableWebcil>
```

See [webcil-packaging-format-for-net-assemblies](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/webassembly?view=aspnetcore-8.0#webcil-packaging-format-for-net-assemblies)

## NuGet
- [![NuGet Badge](https://buildstats.info/nuget/MetadataReferenceService.Abstractions)](https://www.nuget.org/packages/MetadataReferenceService.Abstractions)
- [![NuGet Badge](https://buildstats.info/nuget/MetadataReferenceService.BlazorWasm)](https://www.nuget.org/packages/MetadataReferenceService.BlazorWasm)
- [![NuGet Badge](https://buildstats.info/nuget/MetadataReferenceService.Default)](https://www.nuget.org/packages/MetadataReferenceService.Default)

## Usage
``` c#
// Register
builder.Services.AddSingleton<IMetadataReferenceService, BlazorWasmMetadataReferenceService>();

// Inject
IMetadataReferenceService service = ...;


// Define AssemblyDetails
var assemblyDetails = new AssemblyDetails
{
    Name = "test"
};

// Call CreateAsync
MetadataReference metadataReference = await service.CreateAsync(assemblyDetails);
```


---

## :books: References
- Code based on [LostBeard/BlazorWASMScriptLoader](https://github.com/LostBeard/BlazorWASMScriptLoader)
- [mosse-institute.com/reverse-engineering-portable-executables-pe-part-1](https://library.mosse-institute.com/articles/2022/05/reverse-engineering-portable-executables-pe-part-1/reverse-engineering-portable-executables-pe-part-1.html)
- [mosse-institute.com/reverse-engineering-portable-executables-pe-part-2.html](https://library.mosse-institute.com/articles/2022/05/reverse-engineering-portable-executables-pe-part-2/reverse-engineering-portable-executables-pe-part-2.html)
- [Ellié Computing](http://www.elliecomputing.com) contributes to this project by giving free licences of ECMerge, comparison/merge tool.