using RestEase;
using Shared;

namespace Client8.Services;

[BasePath("api")]
public interface IProcessImageApi
{
    [Post("ProcessImage/{resizeOption}/{processType}/{width}")]
    public Task<Stream> ProcessImageAsync(
        [Body] byte[] image,
        [Path("resizeOption", SerializationMethod = PathSerializationMethod.ToString)] ResizeOption resizeOption,
        [Path("processType", SerializationMethod = PathSerializationMethod.ToString)] ProcessType processType,
        [Path("width")] int? width = null
    );

    [Post("GetImageDetails")]
    public Task<ImageDetails> GetImageDetailsAsync([Body] byte[] image);
}