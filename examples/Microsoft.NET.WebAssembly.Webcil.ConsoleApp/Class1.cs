namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_NT_HEADERS32
{
    public uint Signature;                        // DWORD Signature
    public IMAGE_FILE_HEADER FileHeader;          // IMAGE_FILE_HEADER FileHeader
    public IMAGE_OPTIONAL_HEADER32 OptionalHeader; // IMAGE_OPTIONAL_HEADER32 OptionalHeader
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_NT_HEADERS64
{
    public uint Signature;                        // DWORD Signature
    public IMAGE_FILE_HEADER FileHeader;          // IMAGE_FILE_HEADER FileHeader
    public IMAGE_OPTIONAL_HEADER64 OptionalHeader; // IMAGE_OPTIONAL_HEADER32 OptionalHeader
}

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


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_OPTIONAL_HEADER32
{
    public ushort Magic;
    public byte MajorLinkerVersion;
    public byte MinorLinkerVersion;
    public uint SizeOfCode;
    public uint SizeOfInitializedData;
    public uint SizeOfUninitializedData;

    /// <summary>
    /// A pointer to the entry point function, relative to the image base address.
    /// For executable files, this is the starting address.
    /// For device drivers, this is the address of the initialization function.
    /// The entry point function is optional for DLLs.
    /// When no entry point is present, this member is zero.
    /// </summary>
    public uint AddressOfEntryPoint;

    /// <summary>
    /// A pointer to the beginning of the code section, relative to the image base.
    /// </summary>
    public uint BaseOfCode;

    /// <summary>
    /// A pointer to the beginning of the data section, relative to the image base.
    /// </summary>
    public uint BaseOfData;

    /// <summary>
    /// The preferred address of the first byte of the image when it is loaded in memory.
    /// This value is a multiple of 64K bytes.
    /// The default value for DLLs is 0x10000000.
    /// The default value for applications is 0x00400000, except on Windows CE where it is 0x00010000.
    /// </summary>
    public uint ImageBase;

    public uint SectionAlignment;

    public uint FileAlignment;

    public ushort MajorOperatingSystemVersion;
    public ushort MinorOperatingSystemVersion;
    public ushort MajorImageVersion;
    public ushort MinorImageVersion;
    public ushort MajorSubsystemVersion;
    public ushort MinorSubsystemVersion;
    public uint Win32VersionValue;

    /// <summary>
    /// The size of the image, in bytes, including all headers. Must be a multiple of SectionAlignment.
    /// </summary>
    public uint SizeOfImage;

    /// <summary>
    /// The combined size of the following items, rounded to a multiple of the value specified in the FileAlignment member.
    /// - e_lfanew member of IMAGE_DOS_HEADER
    /// - 4 byte signature
    /// - size of IMAGE_FILE_HEADER
    /// - size of optional header
    /// - size of all section headers
    /// </summary>
    public uint SizeOfHeaders;
    public uint CheckSum;
    public ushort Subsystem;
    public ushort DllCharacteristics;
    public uint SizeOfStackReserve;
    public uint SizeOfStackCommit;
    public uint SizeOfHeapReserve;
    public uint SizeOfHeapCommit;
    public uint LoaderFlags;

    /// <summary>
    /// The number of directory entries in the remainder of the optional header. Each entry describes a location and size.
    /// </summary>
    public uint NumberOfRvaAndSizes;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
    public IMAGE_DATA_DIRECTORY[] DataDirectory;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_OPTIONAL_HEADER64
{
    public ushort Magic;
    public byte MajorLinkerVersion;
    public byte MinorLinkerVersion;
    public uint SizeOfCode;
    public uint SizeOfInitializedData;
    public uint SizeOfUninitializedData;
    public uint AddressOfEntryPoint;
    public uint BaseOfCode;
    public ulong ImageBase;
    public uint SectionAlignment;
    public uint FileAlignment;
    public ushort MajorOperatingSystemVersion;
    public ushort MinorOperatingSystemVersion;
    public ushort MajorImageVersion;
    public ushort MinorImageVersion;
    public ushort MajorSubsystemVersion;
    public ushort MinorSubsystemVersion;
    public uint Win32VersionValue;
    public uint SizeOfImage;
    public uint SizeOfHeaders;
    public uint CheckSum;
    public ushort Subsystem;
    public ushort DllCharacteristics;
    public ulong SizeOfStackReserve;
    public ulong SizeOfStackCommit;
    public ulong SizeOfHeapReserve;
    public ulong SizeOfHeapCommit;
    public uint LoaderFlags;
    public uint NumberOfRvaAndSizes;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)] // IMAGE_NUMBEROF_DIRECTORY_ENTRIES
    public IMAGE_DATA_DIRECTORY[] DataDirectory;
}


[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct IMAGE_DATA_DIRECTORY
{
    public uint VirtualAddress; // DWORD VirtualAddress
    public uint Size;           // DWORD Size
}

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

    //public UnionType Misc;

    /// <summary>
    /// The total size of the section when loaded into memory, in bytes. If this value is greater than the SizeOfRawData member, the section is filled with zeroes.
    /// This field is valid only for executable images and should be set to 0 for object files.
    /// </summary>
    public uint VirtualSize;

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

    //[StructLayout(LayoutKind.Explicit)]
    //public struct UnionType
    //{
    //    [FieldOffset(0)]
    //    public uint PhysicalAddress;

    //    [FieldOffset(0)]
    //    public uint VirtualSize;
    //}
}