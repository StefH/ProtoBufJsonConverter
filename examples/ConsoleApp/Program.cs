using ProtoBufJsonConverter;
using ProtoBufJsonConverter.Models;

namespace ConsoleApp;

public class DynamicProtoLoader
{
    public static void Main()
    {
        var protoDefinition = File.ReadAllText("greet.proto");

        var bytes = Convert.FromBase64String("CgRzdGVm");

        var messageType = "greet.HelloRequest";

        var converter = new Converter();

        var convertToJsonRequest1 = new ConvertToJsonRequest(protoDefinition, messageType, bytes);
        var json1 = converter.ConvertToJson(convertToJsonRequest1);

        var convertToJsonRequest2 = new ConvertToJsonRequest(protoDefinition, messageType, bytes)
            .WithJsonConverterOptions(o => o.WriteIndented = true);
        var json2 = converter.ConvertToJson(convertToJsonRequest2);

        var convertToObjectRequest = new ConvertToObjectRequest(protoDefinition, messageType, bytes);
        var instance = converter.ConvertToObject(convertToObjectRequest);

        var convertToProtoBufRequest1 = new ConvertToProtoBufRequest(protoDefinition, messageType, json1);
        var protobuf1 = converter.ConvertToProtoBuf(convertToProtoBufRequest1);

        //var convertToProtoBufRequest2 = new ConvertToProtoBufRequest(protoDefinition, method, instance);
        //var protobuf2 = converter.ConvertToProtoBuf(convertToProtoBufRequest2);

        var testMessage = new
        {
            Name = "hello"
        };
        var convertToProtoBufRequest3 = new ConvertToProtoBufRequest(protoDefinition, messageType, testMessage);
        var protobuf3 = converter.ConvertToProtoBuf(convertToProtoBufRequest3);

        int x = 9;
    }
}