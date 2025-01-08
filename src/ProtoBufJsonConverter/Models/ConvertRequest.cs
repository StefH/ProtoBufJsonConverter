using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public abstract class ConvertRequest : Request
{
    public string MessageType { get; }

    /// <summary>
    /// Specifies whether the well-known types 'google.protobuf.Timestamp' and 'google.protobuf.TimestampDuration' should be serialized to the newer definition.
    /// </summary>
    public bool SupportNewerGoogleWellKnownTypes { get; protected set; }

    /// <summary>
    /// Create a ConvertRequest.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="messageType">The full type of the protobuf (request/response) message object. Format is "{package-name}.{type-name}".</param>
    /// <param name="protoFileResolver">Provides an optional virtual file system (used for resolving .proto files).</param>
    /// <param name="supportNewerGoogleWellKnownTypes">
    /// Specify whether the well-known types 'google.protobuf.Timestamp' and 'google.protobuf.TimestampDuration' should be serialized to the newer definition.
    /// See also:
    /// - https://protobuf.dev/reference/protobuf/google.protobuf/#timestamp
    /// - https://protobuf.dev/reference/protobuf/google.protobuf/#duration
    ///
    /// Default value is <c>true</c>.
    /// </param>
    protected ConvertRequest(string protoDefinition, string messageType, IProtoFileResolver? protoFileResolver, bool supportNewerGoogleWellKnownTypes = true) : base(protoDefinition, protoFileResolver)
    {
        MessageType = Guard.NotNullOrWhiteSpace(messageType);
        SupportNewerGoogleWellKnownTypes = supportNewerGoogleWellKnownTypes;
    }
}