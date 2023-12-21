using ProtoBufJsonConverter;
using ProtoBufJsonConverter.Models;

namespace ConsoleApp;

public class DynamicProtoLoader
{
    public static void Main()
    {
        var protoDefinition = File.ReadAllText("greet.proto");

        var bytes = Convert.FromBase64String("CgRzdGVm");

        var convertToJsonRequest = new ConvertToJsonRequest(protoDefinition, bytes, "greet.Greeter.SayHello");

        var converter = new Converter();

        var json1 = converter.ConvertToJson(convertToJsonRequest);

        var json2 = converter.ConvertToJson(convertToJsonRequest);

        var convertToProtoBufRequest = new ConvertToProtoBufRequest(protoDefinition, json1, "greet.Greeter.SayHello");
        var protobuf = converter.ConvertToProtoBuf(convertToProtoBufRequest);

        int x = 9;
    }
}