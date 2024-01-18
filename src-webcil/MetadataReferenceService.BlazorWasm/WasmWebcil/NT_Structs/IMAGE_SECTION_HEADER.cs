using System.Runtime.InteropServices;

namespace MetadataReferenceService.BlazorWasm.WasmWebcil.NT_Structs;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_SECTION_HEADER
{
    /// <summary>
    /// An 8-byte, null-padded UTF-8 string.
    /// There is no terminating null character if the string is exactly eight characters long.
    /// For longer names, this member contains a forward slash (/) followed by an ASCII representation of a decimal number that is an offset into the string table.
    /// Executable images do not use a string table and do not support section names longer than eight characters.
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Name;

    public UnionType Misc;

    /// <summary>
    /// The address of the first byte of the section when loaded into memory, relative to the image base.
    /// For object files, this is the address of the first byte before relocation is applied.
    /// </summary>
    public uint VirtualAddress;

    /// <summary>
    /// The size of the initialized data on disk, in bytes.
    /// This value must be a multiple of the FileAlignment member of the IMAGE_OPTIONAL_HEADER structure.
    /// If this value is less than the VirtualSize member, the remainder of the section is filled with zeroes.
    /// If the section contains only uninitialized data, the member is zero.
    /// </summary>
    public uint SizeOfRawData;

    /// <summary>
    /// A file pointer to the first page within the COFF file.
    /// This value must be a multiple of the FileAlignment member of the IMAGE_OPTIONAL_HEADER structure.
    /// If a section contains only uninitialized data, set this member is zero.
    /// </summary>
    public uint PointerToRawData;

    public uint PointerToRelocations;

    public uint PointerToLinenumbers;

    public ushort NumberOfRelocations;

    public ushort NumberOfLinenumbers;

    public uint Characteristics;

    [StructLayout(LayoutKind.Explicit)]
    public struct UnionType
    {
        [FieldOffset(0)]
        public uint PhysicalAddress;

        /// <summary>
        /// The total size of the section when loaded into memory, in bytes. If this value is greater than the SizeOfRawData member, the section is filled with zeroes.
        /// This field is valid only for executable images and should be set to 0 for object files.
        /// </summary>
        [FieldOffset(0)]
        public uint VirtualSize;
    }
}