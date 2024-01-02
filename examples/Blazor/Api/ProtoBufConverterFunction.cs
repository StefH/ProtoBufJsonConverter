using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JsonConverter.Abstractions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using ProtoBufJsonConverter;
using ProtoBufJsonConverter.Models;
using Stef.Validation;

namespace Api;

internal class ProtoBufConverterFunction
{
    private readonly IConverter _protoBufConverter;
    private readonly IJsonConverter _jsonConverter;

    public ProtoBufConverterFunction(IConverter protoBufConverter, IJsonConverter jsonConverter)
    {
        _protoBufConverter = Guard.NotNull(protoBufConverter);
        _jsonConverter = Guard.NotNull(jsonConverter);
    }

    [Function("ConvertToJson")]
    public async Task<string> ConvertToJsonAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, CancellationToken cancellationToken)
    {
        var convertToJsonRequest = await GetRequestAsync<ConvertToJsonRequest>(req, cancellationToken);

        return await _protoBufConverter.ConvertAsync(convertToJsonRequest, cancellationToken).ConfigureAwait(false);
    }

    [Function("ConvertToProtoBuf")]
    public async Task<string> ConvertToProtoBufAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, CancellationToken cancellationToken)
    {
        var convertToProtoBufRequest = await GetRequestAsync<ConvertToProtoBufRequest>(req, cancellationToken);

        var bytes = await _protoBufConverter.ConvertAsync(convertToProtoBufRequest, cancellationToken).ConfigureAwait(false);

        return Convert.ToBase64String(bytes);
    }

    private async Task<T> GetRequestAsync<T>(HttpRequestData req, CancellationToken cancellationToken)
    {
        // Read the request body as a string
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);

        // Deserialize the JSON string into an object
        return _jsonConverter.Deserialize<T>(requestBody);
    }
}