using ProtoBufJsonConverter;
using ProtoBufJsonConverter.Models;

namespace ConsoleApp;

public class DynamicProtoLoader
{
    public static async Task Main()
    {
        var protoDefinition = await File.ReadAllTextAsync("greet.proto");

        var bytes = Convert.FromBase64String("CgRzdGVm");

        var messageType = "greet.HelloRequest";

        var converter = new Converter();

        var convertToJsonRequest1 = new ConvertToJsonRequest(protoDefinition, messageType, bytes);
        var json1 = await converter.ConvertAsync(convertToJsonRequest1);

        var convertToJsonRequest1b = new ConvertToJsonRequest(protoDefinition, messageType, Convert.FromBase64String("AAAAAAYKBHN0ZWY="));
        var json1b = await converter.ConvertAsync(convertToJsonRequest1b);

        var convertToJsonRequest2 = new ConvertToJsonRequest(protoDefinition, messageType, bytes)
            .WithJsonConverterOptions(o => o.WriteIndented = true);
        var json2 = await converter.ConvertAsync(convertToJsonRequest2);

        var convertToObjectRequest = new ConvertToObjectRequest(protoDefinition, messageType, bytes);
        var instance = await converter.ConvertAsync(convertToObjectRequest);

        var convertToProtoBufRequest1 = new ConvertToProtoBufRequest(protoDefinition, messageType, json1);
        var protobuf1 = await converter.ConvertAsync(convertToProtoBufRequest1);

        var convertToProtoBufRequest2 = new ConvertToProtoBufRequest(protoDefinition, messageType, instance);
        var protobuf2 = await converter.ConvertAsync(convertToProtoBufRequest2);

        var testMessage = new
        {
            Name = "hello"
        };
        var convertToProtoBufRequest3 = new ConvertToProtoBufRequest(protoDefinition, messageType, testMessage);
        var protobuf3 = await converter.ConvertAsync(convertToProtoBufRequest3);

        var convertToProtoBufRequestWithGrpcHeader = new ConvertToProtoBufRequest(protoDefinition, messageType, testMessage)
            .WithGrpcHeader(true);
        var protobuf4WithGrpcHeader = await converter.ConvertAsync(convertToProtoBufRequestWithGrpcHeader);

        var person = new
        {
            Name = "stef heyenrath",
            Id = 42,
            Email = "info@mstack.nl"
        };

        var convertToProtoBufRequestPerson = new ConvertToProtoBufRequest(protoDefinition, "greet.Person", person);
        var protobuf3PersonBytes = await converter.ConvertAsync(convertToProtoBufRequestPerson);
        var personAsString = ByteArrayToString(protobuf3PersonBytes);
        int x = 0;
    }

    public static string ByteArrayToString(byte[] byteArray)
    {
        return string.Join(", ", byteArray.Select(b => $"0x{b:X2}"));
    }
}