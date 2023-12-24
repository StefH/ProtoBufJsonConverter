using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public abstract class ConvertRequest
{
    public string ProtoDefinition { get; }

    public string MessageType { get; }

    /// <summary>
    /// Create a ConvertRequest.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="messageType">The full type of the protobuf (request/response) object. Format is "{package-name}.{type-name}".</param>
    protected ConvertRequest(string protoDefinition, string messageType)
    {
        ProtoDefinition = Guard.NotNullOrWhiteSpace(protoDefinition);
        MessageType = Guard.NotNullOrWhiteSpace(messageType);
    }
}