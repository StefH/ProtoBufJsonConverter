using System.Runtime.InteropServices;

namespace MetadataReferenceService.BlazorWasm.WasmWebcil.NT_Structs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_NT_HEADERS64
{
    public uint Signature;                         // DWORD Signature
    public IMAGE_FILE_HEADER FileHeader;           // IMAGE_FILE_HEADER FileHeader
    public IMAGE_OPTIONAL_HEADER64 OptionalHeader; // IMAGE_OPTIONAL_HEADER32 OptionalHeader
}