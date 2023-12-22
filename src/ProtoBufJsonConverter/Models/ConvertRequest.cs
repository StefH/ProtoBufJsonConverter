using JsonConverter.Abstractions;
using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public abstract class ConvertRequest
{
    public string ProtoDefinition { get; }

    public string Method { get; }

    public IJsonConverter? JsonConverter { get; protected set; }

    /// <summary>
    /// Create a ConvertRequest.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="method">The method which is called on service. Format is {package-name}.{service-name}-{method-name}</param>
    protected ConvertRequest(string protoDefinition, string method)
    {
        ProtoDefinition = Guard.NotNullOrWhiteSpace(protoDefinition);
        Method = Guard.NotNullOrWhiteSpace(method);
    }
}