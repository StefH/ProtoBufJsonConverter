using JsonConverter.Abstractions;
using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToJsonRequest : ConvertRequest
{
    public byte[] ProtoBufBytes { get; }

    public JsonConverterOptions? JsonConverterOptions { get; }

    public ConvertToJsonRequest(string protoDefinition, byte[] protoBufBytes, string method, IJsonConverter? jsonConverter = null, JsonConverterOptions? jsonConverterOptions = null) :
        base(protoDefinition, method, jsonConverter)
    {
        Guard.NotNullOrEmpty(protoBufBytes);

        ProtoBufBytes = protoBufBytes;
        JsonConverterOptions = jsonConverterOptions;
    }
}