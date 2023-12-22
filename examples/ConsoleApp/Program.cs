using JsonConverter.Abstractions;
using ProtoBufJsonConverter;
using ProtoBufJsonConverter.Models;

namespace ConsoleApp;

public class DynamicProtoLoader
{
    public static void Main()
    {
        var protoDefinition = File.ReadAllText("greet.proto");

        var bytes = Convert.FromBase64String("CgRzdGVm");

        var method = "greet.Greeter.SayHello";
        
        var converter = new Converter();

        var convertToJsonRequest1 = new ConvertToJsonRequest(protoDefinition, bytes, method);
        var json1 = converter.ConvertToJson(convertToJsonRequest1);
        
        var convertToJsonRequest2 = new ConvertToJsonRequest(protoDefinition, bytes, method)
            .WithJsonConverterOptions(o => o.WriteIndented = true);
        var json2 = converter.ConvertToJson(convertToJsonRequest2);

        var convertToProtoBufRequest = new ConvertToProtoBufRequest(protoDefinition, json1, "greet.Greeter.SayHello");
        var protobuf = converter.ConvertToProtoBuf(convertToProtoBufRequest);

        int x = 9;
    }
}