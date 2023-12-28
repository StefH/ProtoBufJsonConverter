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

    public bool AddGrpcHeader { get; private set; }

    /// <summary>
    /// Create a ConvertToProtoBufRequest to convert a JSON string or object to a ProtoPuf byte array.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="messageType">The full type of the protobuf (request/response) message object. Format is "{package-name}.{type-name}".</param>
    /// <param name="input">The JSON string or Object to convert.</param>
    /// <param name="addGrpcHeader">Add the Grpc Header bytes [default value is false].</param>
    public ConvertToProtoBufRequest(string protoDefinition, string messageType, AnyOf<string, object> input, bool addGrpcHeader = false) : base(protoDefinition, messageType)
    {
        Input = Guard.NotNull(input);
        AddGrpcHeader = addGrpcHeader;
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

    /// <summary>
    /// Add the Grpc Header.
    /// </summary>
    public ConvertToProtoBufRequest WithGrpcHeader()
    {
        AddGrpcHeader = true;
        return this;
    }
}