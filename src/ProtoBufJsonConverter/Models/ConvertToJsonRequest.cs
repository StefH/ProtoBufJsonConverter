using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToJsonRequest
{
    public string ProtoDefinition { get; }

    public byte[] ProtobufBytes { get; }

    public string Method { get; }

    public IJsonConverter JsonConverter { get; }

    /// <summary>
    /// Create a ConvertRequest
    /// </summary>
    /// <param name="protoDefinition"></param>
    /// <param name="protobufBytes"></param>
    /// <param name="method"></param>
    /// <param name="jsonConverter">The optional <see cref="IJsonConverter"/>. Default value is <see cref="NewtonsoftJsonConverter"/>.</param>
    public ConvertToJsonRequest(string protoDefinition, byte[] protobufBytes, string method, IJsonConverter? jsonConverter = null)
    {
        ProtoDefinition = Guard.NotNullOrWhiteSpace(protoDefinition);
        ProtobufBytes = Guard.NotNullOrEmpty(protobufBytes).ToArray();
        Method = Guard.NotNullOrWhiteSpace(method);
        JsonConverter = jsonConverter ?? new NewtonsoftJsonConverter();
    }
}