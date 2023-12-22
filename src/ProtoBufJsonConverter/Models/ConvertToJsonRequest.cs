using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToJsonRequest : ConvertRequest
{
    public byte[] ProtoBufBytes { get; }
    
    public IJsonConverter? JsonConverter { get; protected set; }

    public JsonConverterOptions? JsonConverterOptions { get; private set; }

    /// <summary>
    /// Create a ConvertToJsonRequest.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="method">The method which is called on service. Format is {package-name}.{service-name}-{method-name}</param>
    /// <param name="protoBufBytes">The ProtoBuf byte array to convert.</param>
    public ConvertToJsonRequest(string protoDefinition, string method, byte[] protoBufBytes) : base(protoDefinition, method)
    {
        ProtoBufBytes = Guard.NotNull(protoBufBytes);
    }

    /// <summary>
    /// Set the <see cref="IJsonConverter"/>. Default value is <see cref="NewtonsoftJsonConverter"/>.
    /// </summary>
    /// <param name="jsonConverter">The JsonConverter to use.</param>
    public ConvertToJsonRequest WithJsonConverter(IJsonConverter jsonConverter)
    {
        JsonConverter = Guard.NotNull(jsonConverter);
        return this;
    }

    /// <summary>
    /// Set the <see cref="JsonConverterOptions"/>.
    /// </summary>
    /// <param name="jsonConverterOptions">The JsonConverterOptions to use when serializing an object to a JSON string.</param>
    public ConvertToJsonRequest WithJsonConverterOptions(JsonConverterOptions jsonConverterOptions)
    {
        JsonConverterOptions = Guard.NotNull(jsonConverterOptions);
        return this;
    }

    /// <summary>
    /// Set the <see cref="JsonConverterOptions"/>.
    /// </summary>
    /// <param name="action">The action to configure the JsonConverterOptions to use when serializing an object to a JSON string.</param>
    public ConvertToJsonRequest WithJsonConverterOptions(Action<JsonConverterOptions> action)
    {
        Guard.NotNull(action);

        JsonConverterOptions = new JsonConverterOptions();
        action(JsonConverterOptions);
        return this;
    }
}