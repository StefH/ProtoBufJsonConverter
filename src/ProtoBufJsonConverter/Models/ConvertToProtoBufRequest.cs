﻿using Stef.Validation;

namespace ProtoBufJsonConverter.Models;

public class ConvertToProtoBufRequest : ConvertRequest
{
    public object? Input { get; }

    public bool AddGrpcHeader { get; set; }

    /// <summary>
    /// Create a ConvertToProtoBufRequest to convert a JSON string or an object to a ProtoPuf byte array.
    /// </summary>
    /// <param name="protoDefinition">The proto definition as a string.</param>
    /// <param name="messageType">The full type of the protobuf (request/response) message object. Format is "{package-name}.{type-name}".</param>
    /// <param name="input">The JSON string or object to convert.</param>
    /// <param name="addGrpcHeader">Add the Grpc Header bytes [default is <c>false</c>].</param>
    /// <param name="protoFileResolver">Provides an optional virtual file system (used for resolving .proto files).</param>
    public ConvertToProtoBufRequest(
        string protoDefinition, 
        string messageType, 
        object input, 
        bool addGrpcHeader = false,
        IProtoFileResolver ? protoFileResolver = null
    ) : base(protoDefinition, messageType, protoFileResolver)
    {
        Input = Guard.NotNull(input);
        AddGrpcHeader = addGrpcHeader;
    }

    /// <summary>
    /// Set the Add Grpc Header.
    /// <param name="addHeader">Add the Grpc Header [default is <c>false</c>]</param>
    /// </summary>
    public ConvertToProtoBufRequest WithGrpcHeader(bool addHeader = false)
    {
        AddGrpcHeader = addHeader;
        return this;
    }

    /// <summary>
    /// Set the <see cref="IProtoFileResolver"/>.
    /// </summary>
    /// <param name="protoFileResolver">Provides an optional virtual file system (used for resolving .proto files).</param>
    public ConvertToProtoBufRequest WithProtoFileResolver(IProtoFileResolver protoFileResolver)
    {
        ProtoFileResolver = protoFileResolver;
        return this;
    }
}