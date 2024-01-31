using System.Runtime.InteropServices;

namespace MetadataReferenceService.BlazorWasm.WasmWebcil.NT_Structs;

/// <summary>
/// https://learn.microsoft.com/en-us/windows/win32/api/winnt/ns-winnt-image_data_directory
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct IMAGE_DATA_DIRECTORY
{
    public uint VirtualAddress; // DWORD VirtualAddress
    public uint Size;           // DWORD Size
}