using System.Runtime.InteropServices;

namespace MetadataReferenceService.BlazorWasm.WasmWebcil.NT_Structs;

/// <summary>
/// https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-image_nt_headers32
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct IMAGE_NT_HEADERS32
{
    public uint Signature;                         // DWORD Signature
    public IMAGE_FILE_HEADER FileHeader;           // IMAGE_FILE_HEADER FileHeader
    public IMAGE_OPTIONAL_HEADER32 OptionalHeader; // IMAGE_OPTIONAL_HEADER32 OptionalHeader
}