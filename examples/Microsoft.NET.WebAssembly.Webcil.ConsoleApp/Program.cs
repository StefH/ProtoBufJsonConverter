namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp;

internal class Program
{
    private static void ReadExactly(FileStream s, Span<byte> buffer)
    {
        s.ReadExactly(buffer);
    }

    private static byte[] ULEB128Encode(uint value)
    {
        uint n = value;
        int len = 0;
        do
        {
            n >>= 7;
            len++;
        } while (n != 0);
        byte[] arr = new byte[len];
        int i = 0;
        n = value;
        do
        {
            byte b = (byte)(n & 0x7f);
            n >>= 7;
            if (n != 0)
                b |= 0x80;
            arr[i++] = b;
        } while (n != 0);
        return arr;
    }

    private static uint ULEB128Decode(byte[] bytes)
    {
        uint result = 0;
        int shift = 0;
        foreach (byte b in bytes)
        {
            // Get the value of the current byte without the continuation bit.
            uint current = (uint)(b & 0x7F);
            result |= (current << shift);

            // If the continuation bit is not set, break.
            if ((b & 0x80) == 0)
                break;

            shift += 7;
        }
        return result;
    }


    static void Main(string[] args)
    {
        var e = ULEB128Encode(123456);
        var ee = ULEB128Decode(e);

        var dllStream = new MemoryStream(File.ReadAllBytes(@"c:\temp\ProtoBufJsonConverter.Blazor.dll")); //File.OpenRead(@"c:\temp\ProtoBufJsonConverter.Blazor.dll"));
        var wasmStream = new MemoryStream();
        //var wasmStream = File.OpenWrite(@"c:\temp\wasm.wasm");
        var inputStream = File.OpenRead(@"c:\temp\ProtoBufJsonConverter.Blazor.wasm");
        var outStream = File.OpenWrite(@"c:\temp\ProtoBufJsonConverter.Blazor.dll2");

        new WebcilConverterModified().ConvertToWebcil(dllStream, wasmStream);
        wasmStream.Flush();

        //using WebcilReader reader = new WebcilReader(inputStream);

        //var debug = reader.ReadDebugDirectory();
        //var codeview = debug[0];
        //var checksum = debug[1];
        //var repro = debug[2];

        //var sections = reader._sections;

        //var cel = new MemoryStream();
        //foreach (var webcilSectionHeader in sections)
        //{
        //    var buffer = new byte[webcilSectionHeader.SizeOfRawData];
        //    inputStream.Seek(webcilSectionHeader.PointerToRawData, SeekOrigin.Begin);
        //    ReadExactly(inputStream, buffer);
        //    cel.Write(buffer, 0, buffer.Length);
        //}
        //cel.Flush();
        //cel.Position = 0;


        //new WebcilWasmUnwrapper().WriteWasmUnwrapped(outStream, inputStream);
        //outStream.Flush();


        //var cv = reader.ReadCodeViewDebugDirectoryData(codeview);
        //var cs = reader.ReadPdbChecksumDebugDirectoryData(checksum);

        //MetadataReader mdReader = reader.GetMetadataReader();

        //AssemblyName assemblyName = mdReader.GetAssemblyDefinition().GetAssemblyName();



        //var asm = Assembly.Load(assemblyName);

        //reader.ReadPdbChecksumDebugDirectoryData()

        //var mm = ModuleMetadata.CreateFromImage();

        //var am = AssemblyMetadata.Create();


        //md.GetBlobBytes()

        int y = 0;
    }
}