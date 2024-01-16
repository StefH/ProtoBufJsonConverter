using System.Runtime.InteropServices;

namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp.NT_Structs;

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