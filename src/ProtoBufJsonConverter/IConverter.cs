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
    public Task<string> ConvertAsync(ConvertToJsonRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts a ProtoBuf byte array an object.
    /// </summary>
    /// <param name="request">The ProtoBuf byte array to be converted into JSON.</param>
    /// <param name="cancellationToken">(Optional) A token to monitor for cancellation requests.</param>
    /// <returns>The converted object.</returns>
    public Task<object> ConvertAsync(ConvertToObjectRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts a JSON string or an object to a ProtoBuf byte array format.
    /// </summary>
    /// <param name="request">The JSON string to be converted into a ProtoBuf byte array.</param>
    /// <param name="cancellationToken">(Optional) A token to monitor for cancellation requests.</param>
    /// <returns>A byte array representing the serialized object in ProtoBuf format.</returns>
    public Task<byte[]> ConvertAsync(ConvertToProtoBufRequest request, CancellationToken cancellationToken = default);
}