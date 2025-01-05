namespace ProtoBufJsonConverter.Models;

public class GetInformationRequest : Request
{
    /// <summary>
    /// Create a Request.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="protoFileResolver">Provides an optional virtual file system (used for resolving .proto files).</param>
    public GetInformationRequest(string protoDefinition, IProtoFileResolver? protoFileResolver = null) : base(protoDefinition, protoFileResolver)
    {
    }
}