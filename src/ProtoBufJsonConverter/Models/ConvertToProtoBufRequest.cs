using JsonConverter.Abstractions;
using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToProtoBufRequest : ConvertRequest
{
    public string Json { get; }

    public ConvertToProtoBufRequest(string protoDefinition, string json, string method, IJsonConverter? jsonConverter = null) :
        base(protoDefinition, method, jsonConverter)
    {
        Json = Guard.NotNullOrEmpty(json);
    }
}