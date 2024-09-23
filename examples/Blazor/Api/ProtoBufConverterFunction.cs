using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using ProtoBufJsonConverter;
using ProtoBufJsonConverter.Models;
using Stef.Validation;

namespace Api;

internal class ProtoBufConverterFunction(IConverter protoBufConverter)
{
    private readonly IConverter _protoBufConverter = Guard.NotNull(protoBufConverter);

    [Function("ConvertToJson")]
    public async Task<string> ConvertToJsonAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, CancellationToken cancellationToken)
    {
        var convertToJsonRequest = await GetRequestAsync<ConvertToJsonRequest>(req, cancellationToken);

        return await _protoBufConverter.ConvertAsync(convertToJsonRequest, cancellationToken).ConfigureAwait(false);
    }

    [Function("ConvertToProtoBuf")]
    public async Task<byte[]> ConvertToProtoBufAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, CancellationToken cancellationToken)
    {
        var convertToProtoBufRequest = await GetRequestAsync<ConvertToProtoBufRequest>(req, cancellationToken);

        return await _protoBufConverter.ConvertAsync(convertToProtoBufRequest, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<T> GetRequestAsync<T>(HttpRequestData req, CancellationToken cancellationToken)
    {
        // Read the request body as a string
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync(cancellationToken);

        // Deserialize the JSON string into an object
        return JsonConvert.DeserializeObject<T>(requestBody);
    }
}