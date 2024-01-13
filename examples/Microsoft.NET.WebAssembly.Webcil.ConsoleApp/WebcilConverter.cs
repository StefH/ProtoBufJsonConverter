using System.Buffers.Binary;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ProtoBuf;

namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp;

public class WebcilConverter
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

        var newDllStream = new MemoryStream();

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
        
        var m1 = MetadataReference.CreateFromImage(dllBytes);
        var m2 = MetadataReference.CreateFromImage(newDllBytes);

        var references = RequiredAssemblies.Value.Select(requiredAssembly => MetadataReference.CreateFromFile(requiredAssembly.Location)).Cast<MetadataReference>().ToList();
        references.Add(m2);

        var assemblyName = Path.GetRandomFileName();

        // Parse the source code
        var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText("TestClass1.cs"));

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

            throw new InvalidOperationException($"Unable to compile the code. Errors: {string.Join(",", failures.Select(f => $"{f.Id}-{f.GetMessage()}"))}");
        }

        var a = Assembly.Load(codeStream.ToArray());

        int xxxx = 0;
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct WebcilHeader
    {
        public fixed byte id[4]; // 'W' 'b' 'I' 'L'

        // 4 bytes
        public ushort version_major; // 0

        public ushort version_minor; // 0
        // 8 bytes

        public ushort coff_sections;

        public ushort reserved0; // 0

        // 12 bytes
        public uint pe_cli_header_rva;

        public uint pe_cli_header_size;

        // 20 bytes
        public uint pe_debug_rva;

        public uint pe_debug_size;
        // 28 bytes
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct WebcilSectionHeader
    {
        public readonly int VirtualSize;
        public readonly int VirtualAddress;
        public readonly int SizeOfRawData;
        public readonly int PointerToRawData;

        public WebcilSectionHeader(int virtualSize, int virtualAddress, int sizeOfRawData, int pointerToRawData)
        {
            VirtualSize = virtualSize;
            VirtualAddress = virtualAddress;
            SizeOfRawData = sizeOfRawData;
            PointerToRawData = pointerToRawData;
        }
    }
}