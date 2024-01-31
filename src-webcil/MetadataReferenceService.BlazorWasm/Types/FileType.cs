namespace MetadataReferenceService.BlazorWasm.Types;

internal enum FileType
{
    Dll = 1,

    Wasm = 2,

    Both = Dll | Wasm
}