using MetadataReferenceService.BlazorWasm.WasmWebcil;

namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        // var dllStream = new MemoryStream(File.ReadAllBytes(@"c:\temp\ProtoBufJsonConverter.Blazor.dll")); //File.OpenRead(@"c:\temp\ProtoBufJsonConverter.Blazor.dll"));
        var wasmStream = File.OpenRead(@"c:\temp\ProtoBufJsonConverter.Blazor.wasm");
        
        var peBytes = WebcilConverterUtil.ConvertFromWebcil(wasmStream);
        File.WriteAllBytes(@"c:\temp\ProtoBufJsonConverter.Blazor.dll2", peBytes);
    }
}