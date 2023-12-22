using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToProtoBufRequest : ConvertRequest
{
    public string Json { get; }

    /// <summary>
    /// Create a ConvertToProtoBufRequest.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="method">The method which is called on service. Format is {package-name}.{service-name}-{method-name}</param>
    /// <param name="json">The JSON string to convert.</param>
    public ConvertToProtoBufRequest(string protoDefinition, string method, string json) : base(protoDefinition, method)
    {
        Json = Guard.NotNullOrEmpty(json);
    }

    /// <summary>
    /// Set the <see cref="IJsonConverter"/>. Default value is <see cref="NewtonsoftJsonConverter"/>.
    /// </summary>
    /// <param name="jsonConverter">The JsonConverter to use.</param>
    public ConvertToProtoBufRequest WithJsonConverter(IJsonConverter jsonConverter)
    {
        JsonConverter = Guard.NotNull(jsonConverter);
        return this;
    }
}