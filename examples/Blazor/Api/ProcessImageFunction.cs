using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Shared;
using Stef.Validation;

namespace Api;

internal class ProcessImageFunction
{
    private readonly ImageService _imageService;

    public ProcessImageFunction(ImageService imageService)
    {
        _imageService = Guard.NotNull(imageService);
    }

    [Function("ProcessImage")]
    public async Task<byte[]> ProcessImageAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ProcessImage/{resizeOption:alpha}/{processType}/{width:int?}")] HttpRequestData req,
        string processType,
        string resizeOption,
        int? width,
        CancellationToken cancellationToken
    )
    {
        var request = new CombineRequest
        (
            await GetBytesAsync(req, cancellationToken).ConfigureAwait(false),
            ParseEnum(processType, ProcessType.BlendMStack),
            ParseEnum(resizeOption, ResizeOption.UseSource),
            width
        );

        return await _imageService.CombineAsync(request, cancellationToken).ConfigureAwait(false);
    }

    [Function("GetImageDetails")]
    public async Task<ImageDetails> GetImageDetailsAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req, CancellationToken cancellationToken)
    {
        var bytes = await GetBytesAsync(req, cancellationToken).ConfigureAwait(false);

        return _imageService.GetDetails(bytes);
    }

    private static async Task<byte[]> GetBytesAsync(HttpRequestData req, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await req.Body.CopyToAsync(ms, cancellationToken);
        return ms.ToArray();
    }

    private static T ParseEnum<T>(string value, T defaultValue) where T : struct
    {
        return Enum.TryParse<T>(value, true, out var parsedEnum) ? parsedEnum : defaultValue;
    }
}