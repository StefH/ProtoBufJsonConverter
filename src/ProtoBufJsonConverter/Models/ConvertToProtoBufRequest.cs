using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToProtoBufRequest : ConvertRequest
{
    public string Json { get; }

    /// <inheritdoc />
    public ConvertToProtoBufRequest(string protoDefinition, string json, string method) : base(protoDefinition, method)
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