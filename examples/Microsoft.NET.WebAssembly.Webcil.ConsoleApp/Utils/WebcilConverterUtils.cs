using System.Buffers.Binary;
using System.Collections.Immutable;
using Microsoft.NET.WebAssembly.Webcil.ConsoleApp.NT_Structs;
using System.Runtime.InteropServices;
using Microsoft.NET.WebAssembly.Webcil.ConsoleApp.Extensions;

namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp.Utils;

public static class WebcilConverterUtils
{
    private static readonly byte[] SectionHeaderText = { 0x2E, 0x74, 0x65, 0x78, 0x74, 0x00, 0x00, 0x00 }; // .text
    private static readonly byte[] SectionHeaderRsRc = { 0x2E, 0x72, 0x73, 0x72, 0x63, 0x00, 0x00, 0x00 }; // .rsrc
    private static readonly byte[] SectionHeaderReloc = { 0x2E, 0x72, 0x65, 0x6C, 0x6F, 0x63, 0x00, 0x00 }; // .reloc
    private static readonly byte[] MSDOS =
    {
        0x0E, 0x1F, 0xBA, 0x0E, 0x00, 0xB4, 0x09, 0xCD, 0x21, 0xB8, 0x01, 0x4C, 0xCD, 0x21, 0x54, 0x68,
        0x69, 0x73, 0x20, 0x70, 0x72, 0x6F, 0x67, 0x72, 0x61, 0x6D, 0x20, 0x63, 0x61, 0x6E, 0x6E, 0x6F,
        0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6E, 0x20, 0x69, 0x6E, 0x20, 0x44, 0x4F, 0x53, 0x20,
        0x6D, 0x6F, 0x64, 0x65, 0x2E, 0x0D, 0x0D, 0x0A, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    };

    
    public static byte[] ConvertFromWasmWrappedWebcil(Stream wasmWrappedWebcilStream)
    {
        using var unwrapper = new WasmWebcilUnwrapper(wasmWrappedWebcilStream);

        using var webcilStream = new MemoryStream();
        unwrapper.WriteUnwrapped(webcilStream);
        webcilStream.Flush();
        webcilStream.Seek(0, SeekOrigin.Begin);

        return ConvertFromWebcil(webcilStream);
    }

    public static byte[] ConvertFromWebcil(Stream webcilStream)
    {
        var webcilHeader = ReadHeader(webcilStream);
        var webcilSectionHeaders = ReadSectionHeaders(webcilStream, webcilHeader.coff_sections);
        var webcilSectionHeadersCount = webcilSectionHeaders.Length;
        var webcilSectionHeadersSizeOfRawData = (uint)webcilSectionHeaders.Sum(x => x.SizeOfRawData);

        int sizeofWebCilHeader = Marshal.SizeOf<WebcilHeader>(); // 28
        int sizeofWebCilSectionHeaders = Marshal.SizeOf<WebcilSectionHeader>() * webcilSectionHeadersCount; // 48
        int sizeofIMAGE_DOS_HEADER = Marshal.SizeOf<IMAGE_DOS_HEADER>(); // 64
        int sizeofmsdos_stub = MSDOS.Length; // 64
        int sizeofIMAGE_NT_HEADERS32 = Marshal.SizeOf<IMAGE_NT_HEADERS32>(); // 248
        int sizeofIMAGE_SECTION_HEADER = Marshal.SizeOf<IMAGE_SECTION_HEADER>(); // 40

        int PESectionStart = sizeofIMAGE_DOS_HEADER + sizeofmsdos_stub + sizeofIMAGE_NT_HEADERS32 + webcilSectionHeadersCount * sizeofIMAGE_SECTION_HEADER; // 496
        int PESectionStartRounded = RoundToNearest(PESectionStart);
        byte[] extraBytesAfterSections = new byte[PESectionStartRounded - PESectionStart];

        var pointerToRawDataFirstSectionHeader = webcilSectionHeaders[0].PointerToRawData;
        int pointerToRawDataOffsetBetweenWebcilAndPE = PESectionStartRounded - pointerToRawDataFirstSectionHeader;
        
        using var newDllStream = new MemoryStream();
        var imageDosHeader = new IMAGE_DOS_HEADER
        {
            MagicNumber = 0x5A4D,
            BytesOnLastPageOfFile = 0x90,
            PagesInFile = 3,
            Relocations = 0,
            SizeOfHeaderInParagraphs = 4,
            MinimumExtraParagraphs = 0,
            MaximumExtraParagraphs = 0xFFFF,
            InitialSS = 0,
            InitialSP = 0xB8,
            Checksum = 0,
            InitialIP = 0,
            InitialCS = 0,
            AddressOfRelocationTable = 0x40,
            OverlayNumber = 0,
            ReservedWords1 = new ushort[] { 0, 0, 0, 0 },
            OEMIdentifier = 0,
            OEMInformation = 0,
            ReservedWords2 = new ushort[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            FileAddressOfNewExeHeader = 0x80
        };
        newDllStream.Write(StructToBytes(imageDosHeader));

        newDllStream.Write(MSDOS);

        uint fileAlignment = 0x0200;
        uint sectionAlignment = 0x2000;
        var IMAGE_NT_HEADERS32 = new IMAGE_NT_HEADERS32
        {
            Signature = 0x4550, // 'PE'
            FileHeader = new IMAGE_FILE_HEADER
            {
                Machine = 0x014C,
                NumberOfSections = 3,
                TimeDateStamp = 0xF5192B61,
                PointerToSymbolTable = 0,
                NumberOfSymbols = 0,
                SizeOfOptionalHeader = 0x00E0,
                Characteristics = 0x0022
            },
            OptionalHeader = new IMAGE_OPTIONAL_HEADER32
            {
                Magic = 0x010B, // Signature/Magic - Represents PE32 for 32-bit (0x10b) and PE32+ for 64-bit (0x20B) 
                MajorLinkerVersion = 0x30,
                MinorLinkerVersion = 0,
                SizeOfCode = (uint)webcilSectionHeaders[0].SizeOfRawData,
                SizeOfInitializedData = (uint)(webcilSectionHeaders[1].SizeOfRawData + webcilSectionHeaders[2].SizeOfRawData),
                SizeOfUninitializedData = 0,
                AddressOfEntryPoint = 0, // This can be set to 0
                BaseOfCode = 0x2000,
                BaseOfData = 0xA000,
                ImageBase = 0x400000, // The default value for applications is 0x00400000
                SectionAlignment = sectionAlignment,
                FileAlignment = fileAlignment,
                MajorOperatingSystemVersion = 4,
                MinorOperatingSystemVersion = 0,
                MajorImageVersion = 0,
                MinorImageVersion = 0,
                MajorSubsystemVersion = 4,
                MinorSubsystemVersion = 0,
                Win32VersionValue = 0,
                SizeOfImage = RoundToNearest(webcilSectionHeadersSizeOfRawData, sectionAlignment),
                SizeOfHeaders = GetSizeOfHeaders(imageDosHeader, webcilSectionHeadersCount),
                CheckSum = 0,
                Subsystem = 3, // IMAGE_SUBSYSTEM_WINDOWS_CUI
                DllCharacteristics = 0x8560,
                SizeOfStackReserve = 0x100000,
                SizeOfStackCommit = 0x1000,
                SizeOfHeapReserve = 0x100000,
                SizeOfHeapCommit = 0x1000,
                LoaderFlags = 0,
                NumberOfRvaAndSizes = 0x10,
                DataDirectory = new IMAGE_DATA_DIRECTORY[]
                {
                    new() { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_EXPORT
                    new() { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_IMPORT (can be 0)
                    new() { Size = (uint) webcilSectionHeaders[1].VirtualSize, VirtualAddress = (uint) webcilSectionHeaders[1].VirtualAddress }, // IMAGE_DIRECTORY_ENTRY_RESOURCE
                    new() { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_EXCEPTION
                    new() { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_SECURITY
                    new() { Size = (uint) webcilSectionHeaders[2].VirtualSize, VirtualAddress = (uint) webcilSectionHeaders[2].VirtualAddress }, // IMAGE_DIRECTORY_ENTRY_BASERELOC
                    new() { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_DEBUG (can be 0)
                    new() { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_ARCHITECTURE
                    new() { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_GLOBALPTR
                    new() { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_TLS
                    new() { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG
                    new() { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT
                    new() { Size = 0x0008, VirtualAddress = (uint) webcilSectionHeaders[0].VirtualAddress }, // IMAGE_DIRECTORY_ENTRY_IAT
                    new() { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT
                    new() { Size = 0x0048, VirtualAddress = (uint) webcilSectionHeaders[0].VirtualAddress + 8 }, // TODO ??? IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR
                    new() { Size = 0x0000, VirtualAddress = 0x0000 }  // ?
                }
            }
        };
        newDllStream.Write(StructToBytes(IMAGE_NT_HEADERS32));

        var IMAGE_SECTION_HEADER_text = new IMAGE_SECTION_HEADER
        {
            Name = SectionHeaderText,
            Misc = new IMAGE_SECTION_HEADER.UnionType { VirtualSize = (uint)webcilSectionHeaders[0].VirtualSize },
            VirtualAddress = (uint)webcilSectionHeaders[0].VirtualAddress,
            SizeOfRawData = (uint)webcilSectionHeaders[0].SizeOfRawData,
            PointerToRawData = webcilSectionHeaders[0].GetCorrectedPointerToRawData(pointerToRawDataOffsetBetweenWebcilAndPE),
            Characteristics = 0x60000020
        };
        newDllStream.Write(StructToBytes(IMAGE_SECTION_HEADER_text));

        var IMAGE_SECTION_HEADER_rsrc = new IMAGE_SECTION_HEADER
        {
            Name = SectionHeaderRsRc,
            Misc = new IMAGE_SECTION_HEADER.UnionType { VirtualSize = (uint)webcilSectionHeaders[1].VirtualSize },
            VirtualAddress = (uint)webcilSectionHeaders[1].VirtualAddress,
            SizeOfRawData = (uint)webcilSectionHeaders[1].SizeOfRawData,
            PointerToRawData = webcilSectionHeaders[1].GetCorrectedPointerToRawData(pointerToRawDataOffsetBetweenWebcilAndPE),
            Characteristics = 0x40000040
        };
        newDllStream.Write(StructToBytes(IMAGE_SECTION_HEADER_rsrc));

        var IMAGE_SECTION_HEADER_reloc = new IMAGE_SECTION_HEADER
        {
            Name = SectionHeaderReloc,
            Misc = new IMAGE_SECTION_HEADER.UnionType { VirtualSize = (uint)webcilSectionHeaders[2].VirtualSize },
            VirtualAddress = (uint)webcilSectionHeaders[2].VirtualAddress,
            SizeOfRawData = (uint)webcilSectionHeaders[2].SizeOfRawData,
            PointerToRawData = webcilSectionHeaders[2].GetCorrectedPointerToRawData(pointerToRawDataOffsetBetweenWebcilAndPE),
            Characteristics = 0x42000040
        };
        newDllStream.Write(StructToBytes(IMAGE_SECTION_HEADER_reloc));

        if (extraBytesAfterSections.Length > 0)
        {
            newDllStream.Write(extraBytesAfterSections);
        }

        // Just copy all
        foreach (var webcilSectionHeader in webcilSectionHeaders)
        {
            var buffer = new byte[webcilSectionHeader.SizeOfRawData];
            webcilStream.Seek(webcilSectionHeader.PointerToRawData, SeekOrigin.Begin);
            ReadExactly(webcilStream, buffer);

            newDllStream.Write(buffer, 0, buffer.Length);
        }

        newDllStream.Flush();
        newDllStream.Seek(0, SeekOrigin.Begin);

        return newDllStream.ToArray();
    }

    private static WebcilHeader ReadHeader(Stream s)
    {
        var webcilHeader = ReadStructure<WebcilHeader>(s);

        if (!BitConverter.IsLittleEndian)
        {
            webcilHeader.version_major = BinaryPrimitives.ReverseEndianness(webcilHeader.version_major);
            webcilHeader.version_minor = BinaryPrimitives.ReverseEndianness(webcilHeader.version_minor);
            webcilHeader.coff_sections = BinaryPrimitives.ReverseEndianness(webcilHeader.coff_sections);
            webcilHeader.pe_cli_header_rva = BinaryPrimitives.ReverseEndianness(webcilHeader.pe_cli_header_rva);
            webcilHeader.pe_cli_header_size = BinaryPrimitives.ReverseEndianness(webcilHeader.pe_cli_header_size);
            webcilHeader.pe_debug_rva = BinaryPrimitives.ReverseEndianness(webcilHeader.pe_debug_rva);
            webcilHeader.pe_debug_size = BinaryPrimitives.ReverseEndianness(webcilHeader.pe_debug_size);
        }

        return webcilHeader;
    }

    private static ImmutableArray<WebcilSectionHeader> ReadSectionHeaders(Stream s, int sectionsHeaders)
    {
        var result = new List<WebcilSectionHeader>();
        for (int i = 0; i < sectionsHeaders; i++)
        {
            result.Add(ReadSectionHeader(s));
        }

        return ImmutableArray.Create(result.ToArray());
    }

    private static WebcilSectionHeader ReadSectionHeader(Stream s)
    {
        var sectionHeader = ReadStructure<WebcilSectionHeader>(s);

        if (!BitConverter.IsLittleEndian)
        {
            sectionHeader = new WebcilSectionHeader
            (
                virtualSize: BinaryPrimitives.ReverseEndianness(sectionHeader.VirtualSize),
                virtualAddress: BinaryPrimitives.ReverseEndianness(sectionHeader.VirtualAddress),
                sizeOfRawData: BinaryPrimitives.ReverseEndianness(sectionHeader.SizeOfRawData),
                pointerToRawData: BinaryPrimitives.ReverseEndianness(sectionHeader.PointerToRawData)
            );
        }

        return sectionHeader;
    }

    private static uint GetSizeOfHeaders(IMAGE_DOS_HEADER IMAGE_DOS_HEADER, int numSectionHeaders)
    {
        var soh = IMAGE_DOS_HEADER.FileAddressOfNewExeHeader + // e_lfanew member of IMAGE_DOS_HEADER
                  sizeof(uint) + // 4 byte signature
                  Marshal.SizeOf<IMAGE_FILE_HEADER>() + // size of IMAGE_FILE_HEADER
                  Marshal.SizeOf<IMAGE_OPTIONAL_HEADER32>() + // size of optional header
                  numSectionHeaders * Marshal.SizeOf<IMAGE_SECTION_HEADER>() // size of all section headers
        ;

        return (uint)RoundToNearest(soh);
    }

    public static int RoundToNearest(int number, int nearest = 512)
    {
        var divided = 1.0 * number / nearest;
        var rounded = (int)Math.Round(divided, MidpointRounding.AwayFromZero);
        return rounded * nearest;
    }

    public static uint RoundToNearest(uint number, uint nearest = 512)
    {
        var divided = 1.0 * number / nearest;
        var rounded = (uint)Math.Round(divided, MidpointRounding.AwayFromZero);
        return rounded * nearest;
    }

    private static byte[] StructToBytes<T>(T structData) where T : struct
    {
        int size = Marshal.SizeOf(structData);
        byte[] byteArray = new byte[size];
        nint ptr = Marshal.AllocHGlobal(size);

        try
        {
            Marshal.StructureToPtr(structData, ptr, false);
            Marshal.Copy(ptr, byteArray, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        return byteArray;
    }

    //#if NETCOREAPP2_1_OR_GREATER
    //    private static byte[] StructToBytes<T>(T structure) where T : struct
    //    {
    //        unsafe
    //        {
    //            byte* p = (byte*)&structure;
    //            return new ReadOnlySpan<byte>(p, Marshal.SizeOf<T>()).ToArray();
    //        }
    //    }
    //#else
    //    private static byte[] StructToBytes<T>(T structure) where T : struct
    //    {
    //        int size = Marshal.SizeOf<T>();
    //        byte[] buffer = new byte[size];
    //        IntPtr ptr = IntPtr.Zero;
    //        try
    //        {
    //            ptr = Marshal.AllocHGlobal(size);
    //            Marshal.StructureToPtr(structure, ptr, false);
    //            Marshal.Copy(ptr, buffer, 0, size);
    //        }
    //        finally
    //        {
    //            Marshal.FreeHGlobal(ptr);
    //        }
    //        return buffer;
    //    }
    //#endif

#if NETCOREAPP2_1_OR_GREATER
    private static T ReadStructure<T>(Stream s) where T : unmanaged
    {
        T structure = default;
        unsafe
        {
            byte* p = (byte*)&structure;
            Span<byte> buffer = new Span<byte>(p, sizeof(T));
            int read = s.Read(buffer);
            if (read != sizeof(T))
                throw new InvalidOperationException("Couldn't read the full structure from the stream.");
        }
        return structure;
    }
#else
    private static T ReadStructure<T>(Stream s) where T : unmanaged
    {
        int size = Marshal.SizeOf<T>();
        byte[] buffer = new byte[size];
        s.Read(buffer, 0, size);

        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, ptr, size);
            return Marshal.PtrToStructure<T>(ptr);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
#endif

#if NETCOREAPP2_1_OR_GREATER
    private static void ReadExactly(Stream s, Span<byte> buffer)
    {
        s.ReadExactly(buffer);
    }
#else
    private static void ReadExactly(Stream s, byte[] buffer)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int read = s.Read(buffer, offset, buffer.Length - offset);
            if (read == 0)
                throw new EndOfStreamException();
            offset += read;
        }
    }
#endif
}