using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToJsonRequest : ConvertRequest
{
    public byte[] ProtoBufBytes { get; }

    public bool SkipGrpcHeader { get; private set; }

    public bool WriteIndented { get; private set; }

    /// <summary>
    /// Create a ConvertToJsonRequest.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="messageType">The full type of the protobuf (request/response) message object. Format is "{package-name}.{type-name}".</param>
    /// <param name="protoBufBytes">The ProtoBuf byte array to convert.</param>
    /// <param name="skipGrpcHeader">Skip the Grpc Header bytes [default is <c>true</c>].</param>
    /// <param name="protoFileResolver">Provides an optional virtual file system (used for resolving .proto files).</param>
    /// <param name="supportNewerGoogleWellKnownTypes">
    /// Specify whether the well-known types 'google.protobuf.Timestamp' and 'google.protobuf.TimestampDuration' should be serialized to the newer definition.
    /// See also:
    /// - https://protobuf.dev/reference/protobuf/google.protobuf/#timestamp
    /// - https://protobuf.dev/reference/protobuf/google.protobuf/#duration
    ///
    /// Default value is <c>true</c>.
    /// </param>
    public ConvertToJsonRequest(
        string protoDefinition,
        string messageType,
        byte[] protoBufBytes,
        bool skipGrpcHeader = true,
        IProtoFileResolver? protoFileResolver = null,
        bool supportNewerGoogleWellKnownTypes = true
    ) : base(protoDefinition, messageType, protoFileResolver, supportNewerGoogleWellKnownTypes)
    {
        ProtoBufBytes = Guard.NotNull(protoBufBytes);
        SkipGrpcHeader = skipGrpcHeader;
    }

    /// <summary>
    /// Set the SkipGrpcHeader.
    /// </summary>
    /// <param name="writeIndented">Write the Json as Indented [default is <c>true</c>]</param>
    public ConvertToJsonRequest WithWriteIndented(bool writeIndented = true)
    {
        WriteIndented = writeIndented;
        return this;
    }

    /// <summary>
    /// Set the SkipGrpcHeader.
    /// </summary>
    /// <param name="skipHeader">Skip the Grpc Header [default is <c>true</c>]</param>
    public ConvertToJsonRequest WithSkipGrpcHeader(bool skipHeader)
    {
        SkipGrpcHeader = skipHeader;
        return this;
    }

    /// <summary>
    /// Set the <see cref="IProtoFileResolver"/>.
    /// </summary>
    /// <param name="protoFileResolver">Provides an optional virtual file system (used for resolving .proto files).</param>
    public ConvertToJsonRequest WithProtoFileResolver(IProtoFileResolver protoFileResolver)
    {
        ProtoFileResolver = protoFileResolver;
        return this;
    }

    /// <summary>
    /// Set the support for newer GoogleWellKnownTypes.
    /// </summary>
    /// <param name="supportNewerGoogleWellKnownTypes">
    /// Specify whether the well-known types 'google.protobuf.Timestamp' and 'google.protobuf.TimestampDuration' should be serialized to the newer definition.
    /// See also:
    /// - https://protobuf.dev/reference/protobuf/google.protobuf/#timestamp
    /// - https://protobuf.dev/reference/protobuf/google.protobuf/#duration
    ///
    /// Default value is <c>true</c>.
    /// </param>
    public ConvertToJsonRequest WithSupportNewerGoogleWellKnownTypes(bool supportNewerGoogleWellKnownTypes = true)
    {
        SupportNewerGoogleWellKnownTypes = supportNewerGoogleWellKnownTypes;
        return this;
    }
}