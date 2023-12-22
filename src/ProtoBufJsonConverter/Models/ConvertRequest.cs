using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public abstract class ConvertRequest
{
    public string ProtoDefinition { get; }

    public string Method { get; }

    public IJsonConverter JsonConverter { get; }

    /// <summary>
    /// Create a ConvertRequest
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="method">The method which is called on service. Format is {package-name}.{service-name}-{method-name}</param>
    /// <param name="jsonConverter">The optional <see cref="IJsonConverter"/>. Default value is <see cref="NewtonsoftJsonConverter"/>.</param>
    protected ConvertRequest(string protoDefinition, string method, IJsonConverter? jsonConverter = null)
    {
        ProtoDefinition = Guard.NotNullOrWhiteSpace(protoDefinition);
        Method = Guard.NotNullOrWhiteSpace(method);
        JsonConverter = jsonConverter ?? new NewtonsoftJsonConverter();
    }
}