using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public abstract class Request
{
    public string ProtoDefinition { get; }

    public IProtoFileResolver? ProtoFileResolver { get; protected set; }

    /// <summary>
    /// Create a Request.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="protoFileResolver">Provides an optional virtual file system (used for resolving .proto files).</param>
    protected Request(string protoDefinition, IProtoFileResolver? protoFileResolver = null)
    {
        ProtoDefinition = Guard.NotNullOrWhiteSpace(protoDefinition);
        ProtoFileResolver = protoFileResolver;
    }
}