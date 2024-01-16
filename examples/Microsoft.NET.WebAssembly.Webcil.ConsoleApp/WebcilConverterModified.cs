using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.NET.WebAssembly.Webcil.ConsoleApp.Extensions;
using Microsoft.NET.WebAssembly.Webcil.ConsoleApp.NT_Structs;
using Microsoft.NET.WebAssembly.Webcil.ConsoleApp.Utils;
using ProtoBuf;
using static Microsoft.NET.WebAssembly.Webcil.ConsoleApp.NT_Structs.IMAGE_SECTION_HEADER;

namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp;

public class WebcilConverterModified
{
    public void ConvertToWebcil(MemoryStream inputDllStream, Stream outputWasmStream)
    {
        PEFileInfo peInfo;
        WCFileInfo wcInfo;
        using (var peReader = new PEReader(inputDllStream, PEStreamOptions.LeaveOpen))
        {
            GatherInfo(peReader, out wcInfo, out peInfo);
        }

        // if wrapping in WASM, write the webcil payload to memory because we need to discover the length
        // webcil is about the same size as the PE file
        using var memoryStream = new MemoryStream(checked((int)inputDllStream.Length));
        WriteConversionTo(memoryStream, inputDllStream, peInfo, wcInfo);
        memoryStream.Flush();

        var wrapper = new WebcilWasmWrapper(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        var mem2 = new MemoryStream();
        memoryStream.CopyTo(mem2);
        mem2.Seek(0, SeekOrigin.Begin);
        memoryStream.Seek(0, SeekOrigin.Begin);


        wrapper.WriteWasmWrappedWebcil(outputWasmStream);
        outputWasmStream.Flush();
        outputWasmStream.Seek(0, SeekOrigin.Begin);

        // ---

        var out2 = new MemoryStream();
        outputWasmStream.CopyTo(out2);
        out2.Seek(0, SeekOrigin.Begin);
        outputWasmStream.Seek(0, SeekOrigin.Begin);

        var b = WebcilConverterUtils.ConvertFromWasmWrappedWebcil(out2);
        NewMethod(b);
        return;

        var unwrapper = new WasmWebcilUnwrapper(out2);

        var newS = new MemoryStream();
        unwrapper.WriteUnwrapped(newS);
        newS.Flush();

        var newA = newS.ToArray();
        var origA = mem2.ToArray();

        bool isEqual = origA.SequenceEqual(newA);

        newS.Seek(0, SeekOrigin.Begin);
        var webcilHeader = ReadHeader(newS);
        var webcilSectionHeaders = ReadSectionHeaders(newS, webcilHeader.coff_sections);
        var webcilSectionHeadersCount = webcilSectionHeaders.Length;
        var webcilSectionHeadersSizeOfRawData = (uint) webcilSectionHeaders.Sum(x => x.SizeOfRawData);

        /* PE headers
        Name    VirtualSize VirtualAddress  SizeOfRawData   PointerToRawData    PointerToRelocations;SectionHeader.PointerToLineNumbers;SectionHeader.NumberOfRelocations;SectionHeader.NumberOfLineNumbers;SectionHeader.SectionCharacteristics
        .text   31624       8192            31744           512                 0;0;0;0;"ContainsCode, MemExecute, MemRead"
        .rsrc   1896        40960           2048            32256               0;0;0;0;"ContainsInitializedData, MemRead"
        .reloc  12          49152           512             34304               0;0;0;0;"ContainsInitializedData, MemDiscardable, MemRead"
        */

        /* webcil headers

                VirtualSize VirtualAddress  SizeOfRawData   PointerToRawData
                31624       8192            31744           76
                1896        40960           2048            31820
                12          49152           512             33868
         */

        var msdos_stub = new byte[]
        {
            0x0E, 0x1F, 0xBA, 0x0E, 0x00, 0xB4, 0x09, 0xCD, 0x21, 0xB8, 0x01, 0x4C, 0xCD, 0x21, 0x54, 0x68,
            0x69, 0x73, 0x20, 0x70, 0x72, 0x6F, 0x67, 0x72, 0x61, 0x6D, 0x20, 0x63, 0x61, 0x6E, 0x6E, 0x6F,
            0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6E, 0x20, 0x69, 0x6E, 0x20, 0x44, 0x4F, 0x53, 0x20,
            0x6D, 0x6F, 0x64, 0x65, 0x2E, 0x0D, 0x0D, 0x0A, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        int sizeofWebCilHeader = Marshal.SizeOf<WebcilHeader>(); // 28
        int sizeofWebCilSectionHeaders = Marshal.SizeOf<WebcilSectionHeader>() * webcilSectionHeadersCount; // 48
        int sizeofIMAGE_DOS_HEADER = Marshal.SizeOf<IMAGE_DOS_HEADER>(); // 64
        int sizeofmsdos_stub = msdos_stub.Length; // 64
        int sizeofIMAGE_NT_HEADERS32 = Marshal.SizeOf<IMAGE_NT_HEADERS32>(); // 248
        int sizeofIMAGE_SECTION_HEADER = Marshal.SizeOf<IMAGE_SECTION_HEADER>(); // 40

        int PESectionStart = sizeofIMAGE_DOS_HEADER + sizeofmsdos_stub + sizeofIMAGE_NT_HEADERS32 + webcilSectionHeadersCount * sizeofIMAGE_SECTION_HEADER; // 496
        int PESectionStartRounded = RoundToNearest(PESectionStart);
        byte[] extraBytesAfterSections = new byte[PESectionStartRounded - PESectionStart];

        var PointerToRawDataFirstSectionHeader = webcilSectionHeaders[0].PointerToRawData;
        int PointerToRawDataOffsetBetweenWebcilAndPE = PESectionStartRounded - PointerToRawDataFirstSectionHeader;


        var newDllStream = new MemoryStream();
        var imageDosHeader = new IMAGE_DOS_HEADER
        {
            MagicNumber = 0x5A4D, // The value “MZ” are the initials of the PE designer Mark Zbikowski
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

        newDllStream.Write(msdos_stub);

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
                DataDirectory = new IMAGE_DATA_DIRECTORY[0x10]
                {
                    new IMAGE_DATA_DIRECTORY { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_EXPORT
                    new IMAGE_DATA_DIRECTORY { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_IMPORT (can be 0)
                    new IMAGE_DATA_DIRECTORY { Size = (uint) webcilSectionHeaders[1].VirtualSize, VirtualAddress = (uint) webcilSectionHeaders[1].VirtualAddress }, // IMAGE_DIRECTORY_ENTRY_RESOURCE
                    new IMAGE_DATA_DIRECTORY { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_EXCEPTION
                    new IMAGE_DATA_DIRECTORY { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_SECURITY
                    new IMAGE_DATA_DIRECTORY { Size = (uint) webcilSectionHeaders[2].VirtualSize, VirtualAddress = (uint) webcilSectionHeaders[2].VirtualAddress }, // IMAGE_DIRECTORY_ENTRY_BASERELOC
                    new IMAGE_DATA_DIRECTORY { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_DEBUG (can be 0)
                    new IMAGE_DATA_DIRECTORY { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_ARCHITECTURE
                    new IMAGE_DATA_DIRECTORY { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_GLOBALPTR
                    new IMAGE_DATA_DIRECTORY { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_TLS
                    new IMAGE_DATA_DIRECTORY { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG
                    new IMAGE_DATA_DIRECTORY { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT
                    new IMAGE_DATA_DIRECTORY { Size = 0x0008, VirtualAddress = (uint) webcilSectionHeaders[0].VirtualAddress }, // IMAGE_DIRECTORY_ENTRY_IAT
                    new IMAGE_DATA_DIRECTORY { Size = 0x0000, VirtualAddress = 0x0000 }, // IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT
                    new IMAGE_DATA_DIRECTORY { Size = 0x0048, VirtualAddress = (uint) webcilSectionHeaders[0].VirtualAddress + 8 }, // TODO ??? IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR
                    new IMAGE_DATA_DIRECTORY { Size = 0x0000, VirtualAddress = 0x0000 }  // ?
                }
            }
        };
        newDllStream.Write(StructToBytes(IMAGE_NT_HEADERS32));

        var IMAGE_SECTION_HEADER_text = new IMAGE_SECTION_HEADER
        {
            Name = new byte[] { 0x2E, 0x74, 0x65, 0x78, 0x74, 0x00, 0x00, 0x00 },
            Misc = new UnionType { VirtualSize = (uint)webcilSectionHeaders[0].VirtualSize },
            VirtualAddress = (uint)webcilSectionHeaders[0].VirtualAddress,
            SizeOfRawData = (uint)webcilSectionHeaders[0].SizeOfRawData,
            PointerToRawData = webcilSectionHeaders[0].GetCorrectedPointerToRawData(PointerToRawDataOffsetBetweenWebcilAndPE),
            Characteristics = 0x60000020
        };
        newDllStream.Write(StructToBytes(IMAGE_SECTION_HEADER_text));

        var IMAGE_SECTION_HEADER_rsrc = new IMAGE_SECTION_HEADER
        {
            Name = new byte[] { 0x2E, 0x72, 0x73, 0x72, 0x63, 0x00, 0x00, 0x00 },
            Misc = new UnionType { VirtualSize = (uint)webcilSectionHeaders[1].VirtualSize },
            VirtualAddress = (uint)webcilSectionHeaders[1].VirtualAddress,
            SizeOfRawData = (uint)webcilSectionHeaders[1].SizeOfRawData,
            PointerToRawData = webcilSectionHeaders[1].GetCorrectedPointerToRawData(PointerToRawDataOffsetBetweenWebcilAndPE),
            Characteristics = 0x40000040
        };
        newDllStream.Write(StructToBytes(IMAGE_SECTION_HEADER_rsrc));

        var IMAGE_SECTION_HEADER_reloc = new IMAGE_SECTION_HEADER
        {
            Name = new byte[] { 0x2E, 0x72, 0x65, 0x6C, 0x6F, 0x63, 0x00, 0x00 },
            Misc = new UnionType { VirtualSize = (uint)webcilSectionHeaders[2].VirtualSize },
            VirtualAddress = (uint)webcilSectionHeaders[2].VirtualAddress,
            SizeOfRawData = (uint)webcilSectionHeaders[2].SizeOfRawData,
            PointerToRawData = webcilSectionHeaders[2].GetCorrectedPointerToRawData(PointerToRawDataOffsetBetweenWebcilAndPE),
            Characteristics = 0x42000040
        };
        newDllStream.Write(StructToBytes(IMAGE_SECTION_HEADER_reloc));

        if (extraBytesAfterSections.Length > 0)
        {
            newDllStream.Write(extraBytesAfterSections);
        }

        foreach (var webcilSectionHeader in webcilSectionHeaders)
        {
            var buffer = new byte[webcilSectionHeader.SizeOfRawData];
            newS.Seek(webcilSectionHeader.PointerToRawData, SeekOrigin.Begin);
            ReadExactly(newS, buffer);

            newDllStream.Write(buffer, 0, buffer.Length);
        }
        newDllStream.Flush();
        newDllStream.Seek(0, SeekOrigin.Begin);

        inputDllStream.Seek(0, SeekOrigin.Begin);

        var newDllBytes = newDllStream.ToArray();
        var newDllBytesLen = newDllBytes.Length;
        var dllBytes = inputDllStream.ToArray();
        var dllBytesLen = dllBytes.Length;
        var diff = dllBytesLen - newDllBytesLen;

        bool isEqual2 = dllBytes.SequenceEqual(newDllBytes);

        File.WriteAllBytes(@"c:\temp\new.dll", newDllBytes);

        // var m1 = MetadataReference.CreateFromImage(dllBytes);
        NewMethod(newDllBytes);

        int xxxx = 0;
    }

    private static void NewMethod(byte[] newDllBytes)
    {
        var m2 = MetadataReference.CreateFromImage(newDllBytes);

        var references = RequiredAssemblies.Value
            .Select(requiredAssembly => MetadataReference.CreateFromFile(requiredAssembly.Location))
            .Cast<MetadataReference>().ToList();
        references.Add(m2);

        var assemblyName = Path.GetRandomFileName();

        // Parse the source code
        var code = File.ReadAllText("TestClass1.cs");
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        // Create a compilation
        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Compile the source code
        using var codeStream = new MemoryStream();

        // Wrap this in a task to avoid Blazor Startup Error: System.Threading.SynchronizationLockException: Cannot wait on monitors on this runtime
        var result = compilation.Emit(codeStream);

        if (!result.Success)
        {
            var failures = result
                .Diagnostics
                .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

            throw new InvalidOperationException(
                $"Unable to compile the code. Errors: {string.Join(",", failures.Select(f => $"{f.Id}-{f.GetMessage()}"))}");
        }

        var a = Assembly.Load(codeStream.ToArray());

        var t = Type.GetType("Microsoft.NET.WebAssembly.Webcil.ConsoleApp.TestClass1")!;
        var ins = Activator.CreateInstance(t);
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

    private static uint GetSizeOfHeaders(IMAGE_DOS_HEADER IMAGE_DOS_HEADER, int numSectionHeaders)
    {
        int soh = IMAGE_DOS_HEADER.FileAddressOfNewExeHeader + // e_lfanew member of IMAGE_DOS_HEADER
                  sizeof(uint) + // 4 byte signature
                  Marshal.SizeOf<IMAGE_FILE_HEADER>() + // size of IMAGE_FILE_HEADER
                  Marshal.SizeOf<IMAGE_OPTIONAL_HEADER32>() + // size of optional header
                  numSectionHeaders * Marshal.SizeOf<IMAGE_SECTION_HEADER>() // size of all section headers
            ;

        return (uint) RoundToNearest(soh);
    }

    public static byte[] StructToBytes<T>(T structData) where T : struct
    {
        int size = Marshal.SizeOf(structData);
        byte[] byteArray = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);

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

    private static readonly Lazy<IReadOnlyList<Assembly>> RequiredAssemblies = new(() =>
    {
        var assemblySystemRuntime = Assembly.GetEntryAssembly()!
            .GetReferencedAssemblies()
            .First(a => a.Name == "System.Runtime");

        return new List<Assembly>
        {
            Assembly.Load(assemblySystemRuntime),
            typeof(object).Assembly,
            typeof(ProtoContractAttribute).Assembly
        };
    });

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


    public void WriteConversionTo(Stream outputStream, Stream inputStream, PEFileInfo peInfo, WCFileInfo wcInfo)
    {
        WriteHeader(outputStream, wcInfo.Header);
        WriteSectionHeaders(outputStream, wcInfo.SectionHeaders);
        CopySections(outputStream, inputStream, peInfo.SectionHeaders);
        if (wcInfo.Header.pe_debug_size != 0 && wcInfo.Header.pe_debug_rva != 0)
        {
            var wcDebugDirectoryEntries = FixupDebugDirectoryEntries(peInfo, wcInfo);
            OverwriteDebugDirectoryEntries(outputStream, wcInfo, wcDebugDirectoryEntries);
        }
    }

    public record struct FilePosition(int Position)
    {
        public static implicit operator FilePosition(int position) => new(position);

        public static FilePosition operator +(FilePosition left, int right) => new(left.Position + right);
    }

    private static unsafe int SizeOfHeader()
    {
        return sizeof(WebcilHeader);
    }

    public unsafe void GatherInfo(PEReader peReader, out WCFileInfo wcInfo, out PEFileInfo peInfo)
    {
        var headers = peReader.PEHeaders;
        var peHeader = headers.PEHeader!;
        var coffHeader = headers.CoffHeader;
        var sections = headers.SectionHeaders;

        WebcilHeader header;
        header.id[0] = (byte)'W';
        header.id[1] = (byte)'b';
        header.id[2] = (byte)'I';
        header.id[3] = (byte)'L';
        header.version_major = Constants.WC_VERSION_MAJOR;
        header.version_minor = Constants.WC_VERSION_MINOR;
        header.coff_sections = (ushort)coffHeader.NumberOfSections;
        header.reserved0 = 0;
        header.pe_cli_header_rva = (uint)peHeader.CorHeaderTableDirectory.RelativeVirtualAddress;
        header.pe_cli_header_size = (uint)peHeader.CorHeaderTableDirectory.Size;
        header.pe_debug_rva = (uint)peHeader.DebugTableDirectory.RelativeVirtualAddress;
        header.pe_debug_size = (uint)peHeader.DebugTableDirectory.Size;

        // current logical position in the output file
        FilePosition pos = SizeOfHeader();
        // position of the current section in the output file
        // initially it's after all the section headers
        FilePosition curSectionPos = pos + sizeof(WebcilSectionHeader) * coffHeader.NumberOfSections;
        // The first WC section is immediately after the section directory
        FilePosition firstWCSection = curSectionPos;

        FilePosition firstPESection = 0;

        ImmutableArray<WebcilSectionHeader>.Builder headerBuilder = ImmutableArray.CreateBuilder<WebcilSectionHeader>(coffHeader.NumberOfSections);
        foreach (var sectionHeader in sections)
        {
            // The first section is the one with the lowest file offset
            if (firstPESection.Position == 0)
            {
                firstPESection = sectionHeader.PointerToRawData;
            }
            else
            {
                firstPESection = Math.Min(firstPESection.Position, sectionHeader.PointerToRawData);
            }

            var newHeader = new WebcilSectionHeader
            (
                virtualSize: sectionHeader.VirtualSize,
                virtualAddress: sectionHeader.VirtualAddress,
                sizeOfRawData: sectionHeader.SizeOfRawData,
                pointerToRawData: curSectionPos.Position
            );

            pos += sizeof(WebcilSectionHeader);
            curSectionPos += sectionHeader.SizeOfRawData;
            headerBuilder.Add(newHeader);
        }

        ImmutableArray<DebugDirectoryEntry> debugDirectoryEntries = peReader.ReadDebugDirectory();

        peInfo = new PEFileInfo
        (
            SectionHeaders: sections,
            DebugTableDirectory: peHeader.DebugTableDirectory,
            SectionStart: firstPESection,
            DebugDirectoryEntries: debugDirectoryEntries
        );

        wcInfo = new WCFileInfo
        (
            Header: header,
            SectionHeaders: headerBuilder.MoveToImmutable(),
            SectionStart: firstWCSection
        );
    }

    private static void WriteHeader(Stream s, WebcilHeader webcilHeader)
    {
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

        WriteStructure(s, webcilHeader);
    }

    private static void WriteSectionHeaders(Stream s, ImmutableArray<WebcilSectionHeader> sectionsHeaders)
    {
        foreach (var sectionHeader in sectionsHeaders)
        {
            WriteSectionHeader(s, sectionHeader);
        }
    }

    private static void WriteSectionHeader(Stream s, WebcilSectionHeader sectionHeader)
    {
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

        WriteStructure(s, sectionHeader);
    }

#if NETCOREAPP2_1_OR_GREATER
    private static void WriteStructure<T>(Stream s, T structure)
        where T : unmanaged
    {
        unsafe
        {
            byte* p = (byte*)&structure;
            s.Write(new ReadOnlySpan<byte>(p, sizeof(T)));
        }
    }
#else
    private static void WriteStructure<T>(Stream s, T structure)
        where T : unmanaged
    {
        int size = Marshal.SizeOf<T>();
        byte[] buffer = new byte[size];
        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structure, ptr, false);
            Marshal.Copy(ptr, buffer, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        s.Write(buffer, 0, size);
    }
#endif

    private static void CopySections(Stream outStream, Stream inputStream, ImmutableArray<SectionHeader> peSections)
    {
        // endianness: ok, we're just copying from one stream to another
        foreach (var peHeader in peSections)
        {
            var buffer = new byte[peHeader.SizeOfRawData];
            inputStream.Seek(peHeader.PointerToRawData, SeekOrigin.Begin);
            ReadExactly(inputStream, buffer);
            outStream.Write(buffer, 0, buffer.Length);
        }
    }

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

    private static FilePosition GetPositionOfRelativeVirtualAddress(ImmutableArray<WebcilSectionHeader> wcSections,
        uint relativeVirtualAddress)
    {
        foreach (var section in wcSections)
        {
            if (relativeVirtualAddress >= section.VirtualAddress &&
                relativeVirtualAddress < section.VirtualAddress + section.VirtualSize)
            {
                FilePosition pos = section.PointerToRawData + ((int)relativeVirtualAddress - section.VirtualAddress);
                return pos;
            }
        }

        throw new InvalidOperationException("relative virtual address not in any section");
    }

    // Given a physical file offset, return the section and the offset within the section.
    private (WebcilSectionHeader section, int offset) GetSectionFromFileOffset(
        ImmutableArray<WebcilSectionHeader> peSections, FilePosition fileOffset)
    {
        foreach (var section in peSections)
        {
            if (fileOffset.Position >= section.PointerToRawData &&
                fileOffset.Position < section.PointerToRawData + section.SizeOfRawData)
            {
                return (section, fileOffset.Position - section.PointerToRawData);
            }
        }

        throw new InvalidOperationException($"file offset not in any section (Webcil) for TODO");
    }

    private void GetSectionFromFileOffset(ImmutableArray<SectionHeader> sections, FilePosition fileOffset)
    {
        foreach (var section in sections)
        {
            if (fileOffset.Position >= section.PointerToRawData &&
                fileOffset.Position < section.PointerToRawData + section.SizeOfRawData)
            {
                return;
            }
        }

        throw new InvalidOperationException(
            $"file offset {fileOffset.Position} not in any section (PE) for TODO");
    }

    // Make a new set of debug directory entries that
    // have their data pointers adjusted to be relative to the start of the webcil file.
    // This is necessary because the debug directory entires in the PE file are relative to the start of the PE file,
    // and a PE header is bigger than a webcil header.
    private ImmutableArray<DebugDirectoryEntry> FixupDebugDirectoryEntries(PEFileInfo peInfo, WCFileInfo wcInfo)
    {
        int dataPointerAdjustment = peInfo.SectionStart.Position - wcInfo.SectionStart.Position;
        ImmutableArray<DebugDirectoryEntry> entries = peInfo.DebugDirectoryEntries;
        ImmutableArray<DebugDirectoryEntry>.Builder newEntries =
            ImmutableArray.CreateBuilder<DebugDirectoryEntry>(entries.Length);
        foreach (var entry in entries)
        {
            DebugDirectoryEntry newEntry;
            if (entry.Type == DebugDirectoryEntryType.Reproducible || entry.DataPointer == 0 || entry.DataSize == 0)
            {
                // this entry doesn't have an associated data pointer, so just copy it
                newEntry = entry;
            }
            else
            {
                // the "DataPointer" field is a file offset in the PE file, adjust the entry wit the corresponding offset in the Webcil file
                var newDataPointer = entry.DataPointer - dataPointerAdjustment;
                newEntry = new DebugDirectoryEntry(entry.Stamp, entry.MajorVersion, entry.MinorVersion, entry.Type,
                    entry.DataSize, entry.DataRelativeVirtualAddress, newDataPointer);
                GetSectionFromFileOffset(peInfo.SectionHeaders, entry.DataPointer);
                // validate that the new entry is in some section
                GetSectionFromFileOffset(wcInfo.SectionHeaders, newDataPointer);
            }

            newEntries.Add(newEntry);
        }

        return newEntries.MoveToImmutable();
    }

    private static void OverwriteDebugDirectoryEntries(Stream s, WCFileInfo wcInfo,
        ImmutableArray<DebugDirectoryEntry> entries)
    {
        FilePosition debugDirectoryPos =
            GetPositionOfRelativeVirtualAddress(wcInfo.SectionHeaders, wcInfo.Header.pe_debug_rva);
        using var writer = new BinaryWriter(s, System.Text.Encoding.UTF8, leaveOpen: true);
        writer.Seek(debugDirectoryPos.Position, SeekOrigin.Begin);
        foreach (var entry in entries)
        {
            WriteDebugDirectoryEntry(writer, entry);
        }
        // TODO check that we overwrite with the same size as the original

        // restore the stream position
        writer.Seek(0, SeekOrigin.End);
    }

    private static void WriteDebugDirectoryEntry(BinaryWriter writer, DebugDirectoryEntry entry)
    {
        writer.Write((uint)0); // Characteristics
        writer.Write(entry.Stamp);
        writer.Write(entry.MajorVersion);
        writer.Write(entry.MinorVersion);
        writer.Write((uint)entry.Type);
        writer.Write(entry.DataSize);
        writer.Write(entry.DataRelativeVirtualAddress);
        writer.Write(entry.DataPointer);
    }

    // Interesting stuff we've learned about the input PE file
    public record PEFileInfo(
        // The sections in the PE file
        ImmutableArray<SectionHeader> SectionHeaders,
        // The location of the debug directory entries
        DirectoryEntry DebugTableDirectory,
        // The file offset of the sections, following the section directory
        FilePosition SectionStart,
        // The debug directory entries
        ImmutableArray<DebugDirectoryEntry> DebugDirectoryEntries
    );

    // Intersting stuff we know about the webcil file we're writing
    public record WCFileInfo(
        // The header of the webcil file
        WebcilHeader Header,
        // The section directory of the webcil file
        ImmutableArray<WebcilSectionHeader> SectionHeaders,
        // The file offset of the sections, following the section directory
        FilePosition SectionStart
    );

    internal static unsafe class Constants
    {
        public const int WC_VERSION_MAJOR = 0;
        public const int WC_VERSION_MINOR = 0;
    }

    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    //public unsafe struct WebcilHeader
    //{
    //    public fixed byte id[4]; // 'W' 'b' 'I' 'L'

    //    // 4 bytes
    //    public ushort version_major; // 0

    //    public ushort version_minor; // 0
    //    // 8 bytes

    //    public ushort coff_sections;

    //    public ushort reserved0; // 0

    //    // 12 bytes
    //    public uint pe_cli_header_rva;

    //    public uint pe_cli_header_size;

    //    // 20 bytes
    //    public uint pe_debug_rva;

    //    public uint pe_debug_size;
    //    // 28 bytes
    //}

    //[StructLayout(LayoutKind.Sequential, Pack = 1)]
    //public readonly struct WebcilSectionHeader
    //{
    //    public readonly int VirtualSize;
    //    public readonly int VirtualAddress;
    //    public readonly int SizeOfRawData;
    //    public readonly int PointerToRawData;

    //    public WebcilSectionHeader(int virtualSize, int virtualAddress, int sizeOfRawData, int pointerToRawData)
    //    {
    //        VirtualSize = virtualSize;
    //        VirtualAddress = virtualAddress;
    //        SizeOfRawData = sizeOfRawData;
    //        PointerToRawData = pointerToRawData;
    //    }
    //}

    // stef
    //private static uint ReadULEB128(Stream stream)
    //{
    //    uint result = 0;
    //    int shift = 0;
    //    byte byteVal;

    //    do
    //    {
    //        byteVal = (byte)stream.ReadByte();
    //        result |= (uint)(byteVal & 0x7F) << shift;
    //        shift += 7;
    //    } while ((byteVal & 0x80) != 0);

    //    return result;
    //}
}