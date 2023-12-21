using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverter;

public interface IConverter
{
    public string ConvertToJson(ConvertToJsonRequest request, CancellationToken cancellationToken = default);

    public byte[] ConvertToProtoBuf(ConvertToProtoBufRequest request, CancellationToken cancellationToken = default);
}