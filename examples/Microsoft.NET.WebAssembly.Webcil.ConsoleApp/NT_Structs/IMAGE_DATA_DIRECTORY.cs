using System.Runtime.InteropServices;

namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp.NT_Structs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_DATA_DIRECTORY
{
    public uint VirtualAddress; // DWORD VirtualAddress
    public uint Size;           // DWORD Size
}