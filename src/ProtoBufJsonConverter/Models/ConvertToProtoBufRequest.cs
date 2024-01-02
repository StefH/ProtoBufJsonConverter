using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
using Newtonsoft.Json;
using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToProtoBufRequest : ConvertRequest
{
    [JsonIgnore]
    public IJsonConverter? JsonConverter { get; private set; }

    public JsonConverterOptions? JsonConverterOptions { get; set; }

    public object? Input { get; }

    public bool AddGrpcHeader { get; set; }

    /// <summary>
    /// Create a ConvertToProtoBufRequest to convert a JSON string or an object to a ProtoPuf byte array.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="messageType">The full type of the protobuf (request/response) message object. Format is "{package-name}.{type-name}".</param>
    /// <param name="input">The JSON string or object to convert.</param>
    /// <param name="addGrpcHeader">Add the Grpc Header bytes [default is <c>false</c>].</param>
    public ConvertToProtoBufRequest(string protoDefinition, string messageType, object input, bool addGrpcHeader = false) : base(protoDefinition, messageType)
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
    /// Set the Add Grpc Header.
    /// <param name="addHeader">Add the Grpc Header [default is <c>false</c>]</param>
    /// </summary>
    public ConvertToProtoBufRequest WithGrpcHeader(bool addHeader = false)
    {
        AddGrpcHeader = addHeader;
        return this;
    }
}