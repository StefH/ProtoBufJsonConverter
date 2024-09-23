using ProtoBufJsonConverter;
using ProtoBufJsonConverter.Models;

namespace ConsoleApp;

public class DynamicProtoLoader
{
    private class MyProtoFileResolver : IProtoFileResolver
    {
        public bool Exists(string path)
        {
            return true;
        }

        public TextReader OpenText(string path)
        {
            return File.OpenText(path);
        }
    }

    private static string AppendDummyComment() => "\r\n// Dummy" + Guid.NewGuid();

    public static async Task Main()
    {
        var protoDefinition = await File.ReadAllTextAsync("greet.proto");

        var bytes = Convert.FromBase64String("CgRzdGVm");

        var messageType = "greet.Other";

        var converter0 = new Converter(new MyProtoFileResolver());

        var convertToJsonRequest0 = new ConvertToJsonRequest(protoDefinition + AppendDummyComment(), messageType, bytes);
        var json0 = await converter0.ConvertAsync(convertToJsonRequest0);

        var converter00 = new Converter();

        var convertToJsonRequest00 = new ConvertToJsonRequest(protoDefinition + AppendDummyComment(), messageType, bytes)
            .WithProtoFileResolver(new MyProtoFileResolver());
        var json00 = await converter00.ConvertAsync(convertToJsonRequest00);

        var convertToObjectRequest00 = new ConvertToObjectRequest(protoDefinition + AppendDummyComment(), messageType, bytes);
        var instance00 = await converter00.ConvertAsync(convertToObjectRequest00);


        var converter = new Converter();

        var convertToJsonRequest1 = new ConvertToJsonRequest(protoDefinition + AppendDummyComment(), messageType, bytes);
        var json1 = await converter.ConvertAsync(convertToJsonRequest1);

        var convertToJsonRequest1b = new ConvertToJsonRequest(protoDefinition + AppendDummyComment(), messageType, Convert.FromBase64String("AAAAAAYKBHN0ZWY="));
        var json1b = await converter.ConvertAsync(convertToJsonRequest1b);

        var convertToJsonRequest2 = new ConvertToJsonRequest(protoDefinition + AppendDummyComment(), messageType, bytes)
           .WithWriteIndented();
        var json2 = await converter.ConvertAsync(convertToJsonRequest2);

        var convertToObjectRequest = new ConvertToObjectRequest(protoDefinition + AppendDummyComment(), messageType, bytes);
        var instance = await converter.ConvertAsync(convertToObjectRequest);

        var convertToProtoBufRequest1 = new ConvertToProtoBufRequest(protoDefinition + AppendDummyComment(), messageType, json1);
        var protobuf1 = await converter.ConvertAsync(convertToProtoBufRequest1);

        var convertToProtoBufRequest2 = new ConvertToProtoBufRequest(protoDefinition + AppendDummyComment(), messageType, instance);
        var protobuf2 = await converter.ConvertAsync(convertToProtoBufRequest2);

        var testMessage = new
        {
            Name = "hello"
        };
        var convertToProtoBufRequest3 = new ConvertToProtoBufRequest(protoDefinition + AppendDummyComment(), messageType, testMessage);
        var protobuf3 = await converter.ConvertAsync(convertToProtoBufRequest3);

        var convertToProtoBufRequestWithGrpcHeader = new ConvertToProtoBufRequest(protoDefinition + AppendDummyComment(), messageType, testMessage)
            .WithGrpcHeader(true);
        var protobuf4WithGrpcHeader = await converter.ConvertAsync(convertToProtoBufRequestWithGrpcHeader);

        var person = new
        {
            Name = "stef heyenrath",
            Id = 42,
            Email = "info@mstack.nl"
        };

        var convertToProtoBufRequestPerson = new ConvertToProtoBufRequest(protoDefinition + AppendDummyComment(), "greet.Person", person);
        var protobuf3PersonBytes = await converter.ConvertAsync(convertToProtoBufRequestPerson);
        var personAsString = ByteArrayToString(protobuf3PersonBytes);
    }

    public static string ByteArrayToString(byte[] byteArray)
    {
        return string.Join(", ", byteArray.Select(b => $"0x{b:X2}"));
    }
}