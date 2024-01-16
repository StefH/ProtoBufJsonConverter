using System.Runtime.InteropServices;

namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp.NT_Structs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_FILE_HEADER
{
    public ushort Machine;               // WORD Machine
    public ushort NumberOfSections;      // WORD NumberOfSections
    public uint TimeDateStamp;           // DWORD TimeDateStamp
    public uint PointerToSymbolTable;    // DWORD PointerToSymbolTable
    public uint NumberOfSymbols;         // DWORD NumberOfSymbols
    public ushort SizeOfOptionalHeader;  // WORD SizeOfOptionalHeader
    public ushort Characteristics;       // WORD Characteristics
}