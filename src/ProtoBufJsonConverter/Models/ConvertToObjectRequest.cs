﻿using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToObjectRequest : ConvertRequest
{
    public byte[] ProtoBufBytes { get; }

    public bool SkipGrpcHeader { get; }

    /// <summary>
    /// Create a ConvertToJsonRequest.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="messageType">The full type of the protobuf (request/response) message object. Format is "{package-name}.{type-name}".</param>
    /// <param name="protoBufBytes">The ProtoBuf byte array to convert.</param>
    /// <param name="skipGrpcHeader">Skip the Grpc Header bytes [default is <c>true</c>].</param>
    public ConvertToObjectRequest(
        string protoDefinition, 
        string messageType, 
        byte[] protoBufBytes,
        bool skipGrpcHeader = true
    ) : base(protoDefinition, messageType)
    {
        ProtoBufBytes = Guard.NotNull(protoBufBytes);
        SkipGrpcHeader = skipGrpcHeader;
    }
}