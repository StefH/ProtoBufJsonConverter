using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToObjectRequest : ConvertRequest
{
    public byte[] ProtoBufBytes { get; }
    
    /// <summary>
    /// Create a ConvertToJsonRequest.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="method">The method which is called on service. Format is "{package-name}.{service-name}-{method-name}".</param>
    /// <param name="protoBufBytes">The ProtoBuf byte array to convert.</param>
    public ConvertToObjectRequest(string protoDefinition, string method, byte[] protoBufBytes) : base(protoDefinition, method)
    {
        ProtoBufBytes = Guard.NotNull(protoBufBytes);
    }
}