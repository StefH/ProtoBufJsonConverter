using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public abstract class ConvertRequest
{
    public string ProtoDefinition { get; }

    public string MessageType { get; }

    public IProtoFileResolver? ProtoFileResolver { get; protected set; }

    /// <summary>
    /// Create a ConvertRequest.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="messageType">The full type of the protobuf (request/response) message object. Format is "{package-name}.{type-name}".</param>
    /// <param name="protoFileResolver">Provides an optional virtual file system (used for resolving .proto files).</param>
    protected ConvertRequest(string protoDefinition, string messageType, IProtoFileResolver? protoFileResolver)
    {
        ProtoDefinition = Guard.NotNullOrWhiteSpace(protoDefinition);
        MessageType = Guard.NotNullOrWhiteSpace(messageType);
        ProtoFileResolver = protoFileResolver;
    }
}