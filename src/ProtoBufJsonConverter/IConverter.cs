using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverter;

public interface IConverter
{
    /// <summary>
    /// Converts a ProtoBuf byte array to JSON format.
    /// </summary>
    /// <param name="request">The ProtoBuf byte array to be converted into JSON.</param>
    /// <param name="cancellationToken">(Optional) A token to monitor for cancellation requests.</param>
    /// <returns>A string representing the converted object in JSON format.</returns>
    public string ConvertToJson(ConvertToJsonRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts a JSON string to a ProtoBuf byte array format.
    /// </summary>
    /// <param name="request">The JSON string to be converted into a ProtoBuf byte array.</param>
    /// <param name="cancellationToken">(Optional) A token to monitor for cancellation requests.</param>
    /// <returns>A byte array representing the serialized object in ProtoBuf format.</returns>
    public byte[] ConvertToProtoBuf(ConvertToProtoBufRequest request, CancellationToken cancellationToken = default);
}