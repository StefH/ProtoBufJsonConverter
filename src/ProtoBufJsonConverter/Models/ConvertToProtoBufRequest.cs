using AnyOfTypes;
using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToProtoBufRequest : ConvertRequest
{
    public IJsonConverter? JsonConverter { get; private set; }

    public JsonConverterOptions? JsonConverterOptions { get; private set; }

    public AnyOf<string, object> Input { get; }

    /// <summary>
    /// Create a ConvertToProtoBufRequest to convert a JSON string or object to a ProtoPuf byte array.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="method">The method which is called on service. Format is "{package-name}.{service-name}-{method-name}".</param>
    /// <param name="input">The JSON string or Object to convert.</param>
    public ConvertToProtoBufRequest(string protoDefinition, string method, AnyOf<string, object> input) : base(protoDefinition, method)
    {
        Input = Guard.NotNull(input);
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

    /// <summary>
    /// Set the <see cref="JsonConverterOptions"/>.
    /// </summary>
    /// <param name="jsonConverterOptions">The JsonConverterOptions to use when serializing an object to a JSON string.</param>
    public ConvertToProtoBufRequest WithJsonConverterOptions(JsonConverterOptions jsonConverterOptions)
    {
        JsonConverterOptions = Guard.NotNull(jsonConverterOptions);
        return this;
    }
}