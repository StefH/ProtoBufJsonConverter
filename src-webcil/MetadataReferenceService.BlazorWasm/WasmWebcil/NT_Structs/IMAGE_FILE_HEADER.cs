using System.Runtime.InteropServices;

namespace MetadataReferenceService.BlazorWasm.WasmWebcil.NT_Structs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_FILE_HEADER
{
    /// <summary>
    /// The architecture type of the computer.
    /// An image file can only be run on the specified computer or a system that emulates the specified computer.
    /// </summary>
    public ushort Machine;

    /// <summary>
    /// The number of sections.
    /// This indicates the size of the section table, which immediately follows the headers.
    /// Note that the Windows loader limits the number of sections to 96.
    /// </summary>
    public ushort NumberOfSections;

    /// <summary>
    /// The low 32 bits of the time stamp of the image.
    /// This represents the date and time the image was created by the linker.
    /// The value is represented in the number of seconds elapsed since midnight (00:00:00), January 1, 1970, Universal Coordinated Time, according to the system clock.
    /// </summary>
    public uint TimeDateStamp;

    public uint PointerToSymbolTable;

    public uint NumberOfSymbols;

    public ushort SizeOfOptionalHeader;

    public ushort Characteristics;
}