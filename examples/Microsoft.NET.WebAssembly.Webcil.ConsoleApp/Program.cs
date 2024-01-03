namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var s = File.OpenRead(@"c:\temp\ProtoBufJsonConverter.Blazor.wasm");

        using WebcilReader reader = new WebcilReader(s);

        

//        var x = reader.ReadDebugDirectory()[0];

        var md = reader.GetMetadataReader();


        
        //md.GetBlobBytes()

        int y = 0;
    }
}