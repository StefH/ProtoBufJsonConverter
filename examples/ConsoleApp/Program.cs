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

        var convertToJsonRequest1 = new ConvertToJsonRequest(protoDefinition, method, bytes);
        var json1 = converter.ConvertToJson(convertToJsonRequest1);

        var convertToJsonRequest2 = new ConvertToJsonRequest(protoDefinition, method, bytes)
            .WithJsonConverterOptions(o => o.WriteIndented = true);
        var json2 = converter.ConvertToJson(convertToJsonRequest2);

        var convertToObjectRequest = new ConvertToObjectRequest(protoDefinition, method, bytes);
        var instance = converter.ConvertToObject(convertToObjectRequest);

        var convertToProtoBufRequest1 = new ConvertToProtoBufRequest(protoDefinition, "greet.Greeter.SayHello", json1);
        var protobuf1 = converter.ConvertToProtoBuf(convertToProtoBufRequest1);

        var convertToProtoBufRequest2 = new ConvertToProtoBufRequest(instance);
        var protobuf2 = converter.ConvertToProtoBuf(convertToProtoBufRequest2);

        var testMessage = new TestMessage
        {
            Message = "hello"
        };
        var convertToProtoBufRequest3 = new ConvertToProtoBufRequest(testMessage);
        var protobuf3 = converter.ConvertToProtoBuf(convertToProtoBufRequest3);

        int x = 9;
    }
}