namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var s = File.OpenRead(@"c:\temp\ProtoBufJsonConverter.Blazor.wasm");

        var wc = WebcilConverter.FromPortableExecutable(@"c:\temp\ProtoBufJsonConverter.Blazor.dll", @"c:\temp\wasm.wasm");
        wc.ConvertToWebcil();


     //   WasmFile file = WasmFile.ReadBinary(s);

        using WebcilReader reader = new WebcilReader(s);



        //        var x = reader.ReadDebugDirectory()[0];

        var md = reader.GetMetadataReader();

        //reader.ReadPdbChecksumDebugDirectoryData()

        //var mm = ModuleMetadata.CreateFromImage();

        //var am = AssemblyMetadata.Create();

        
        //md.GetBlobBytes()

        int y = 0;
    }
}