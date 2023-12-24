using AnyOfTypes;
using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToProtoBufRequest
{
    public IJsonConverter? JsonConverter { get; protected set; }

    public string? ProtoDefinition { get; }

    public string? Method { get; }

    public AnyOf<string, object> Input { get; }

    /// <summary>
    /// Create a ConvertToProtoBufRequest to convert an object to a ProtoPuf byte array.
    /// </summary>
    /// <param name="input">The Object to convert. Note that this object must extend <see cref="ProtoBuf.IExtensible"/>.</param>
    public ConvertToProtoBufRequest(object input)
    {
        Input = Guard.NotNull(input);
    }

    /// <summary>
    /// Create a ConvertToProtoBufRequest to convert a JSON string to a ProtoPuf byte array.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="method">The method which is called on service. Format is "{package-name}.{service-name}-{method-name}".</param>
    /// <param name="json">The JSON string or Object to convert.</param>
    public ConvertToProtoBufRequest(string protoDefinition, string method, string json)
    {
        ProtoDefinition = Guard.NotNullOrWhiteSpace(protoDefinition);
        Method = Guard.NotNullOrWhiteSpace(method);
        Input = Guard.NotNull(json);
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