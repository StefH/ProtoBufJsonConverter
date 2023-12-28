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
        var json1 = converter.Convert(convertToJsonRequest1);

        var convertToJsonRequest1b = new ConvertToJsonRequest(protoDefinition, messageType, Convert.FromBase64String("AAAAAAYKBHN0ZWY="));
        var json1b = converter.Convert(convertToJsonRequest1b);

        var convertToJsonRequest2 = new ConvertToJsonRequest(protoDefinition, messageType, bytes)
            .WithJsonConverterOptions(o => o.WriteIndented = true);
        var json2 = converter.Convert(convertToJsonRequest2);

        var convertToObjectRequest = new ConvertToObjectRequest(protoDefinition, messageType, bytes);
        var instance = converter.Convert(convertToObjectRequest);

        var convertToProtoBufRequest1 = new ConvertToProtoBufRequest(protoDefinition, messageType, json1);
        var protobuf1 = converter.Convert(convertToProtoBufRequest1);

        var convertToProtoBufRequest2 = new ConvertToProtoBufRequest(protoDefinition, messageType, instance);
        var protobuf2 = converter.Convert(convertToProtoBufRequest2);

        var testMessage = new
        {
            Name = "hello"
        };
        var convertToProtoBufRequest3 = new ConvertToProtoBufRequest(protoDefinition, messageType, testMessage);
        var protobuf3 = converter.Convert(convertToProtoBufRequest3);

        var convertToProtoBufRequestWithGrpcHeader = new ConvertToProtoBufRequest(protoDefinition, messageType, testMessage)
            .WithGrpcHeader();
        var protobuf4WithGrpcHeader = converter.Convert(convertToProtoBufRequestWithGrpcHeader);

        int x = 9;
    }
}