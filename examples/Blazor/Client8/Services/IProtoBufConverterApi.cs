using ProtoBufJsonConverter.Models;
using RestEase;

namespace Client8.Services;

[BasePath("api")]
public interface IProtoBufConverterApi
{
    [Post("ConvertToJson")]
    public Task<string> ConvertToJsonAsync([Body] ConvertToJsonRequest request, CancellationToken cancellationToken = default);

    [Post("ConvertToProtoBuf")]
    public Task<string> ConvertToProtoBufAsync([Body] ConvertToProtoBufRequest request, CancellationToken cancellationToken = default);
}