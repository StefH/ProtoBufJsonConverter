using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToProtoBufRequest
{
    public string ProtoDefinition { get; }

    public string Json { get; }

    public string Method { get; }

    public IJsonConverter JsonConverter { get; }

    /// <summary>
    /// Create a ConvertRequest
    /// </summary>
    /// <param name="protoDefinition"></param>
    /// <param name="json"></param>
    /// <param name="method"></param>
    /// <param name="jsonConverter">The optional <see cref="IJsonConverter"/>. Default value is <see cref="NewtonsoftJsonConverter"/>.</param>
    public ConvertToProtoBufRequest(string protoDefinition, string json, string method, IJsonConverter? jsonConverter = null)
    {
        ProtoDefinition = Guard.NotNullOrWhiteSpace(protoDefinition);
        Json = Guard.NotNullOrEmpty(json);
        Method = Guard.NotNullOrWhiteSpace(method);
        JsonConverter = jsonConverter ?? new NewtonsoftJsonConverter();
    }
}