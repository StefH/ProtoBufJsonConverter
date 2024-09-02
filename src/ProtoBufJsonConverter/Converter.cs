﻿using System.Collections.Concurrent;
using System.Reflection;
using Google.Protobuf.Reflection;
using MetadataReferenceService.Abstractions;
using MetadataReferenceService.Default;
using ProtoBuf.Reflection;
using ProtoBufJsonConverter.Extensions;
using ProtoBufJsonConverter.IO;
using ProtoBufJsonConverter.Models;
using ProtoBufJsonConverter.Utils;
using Stef.Validation;

namespace ProtoBufJsonConverter;

/// <summary>
/// The Converter
/// </summary>
public class Converter : IConverter
{
    private static readonly Dictionary<string, string> CodeGenerateOptions = new()
    {
        { "nullablevaluetype", "true" }
    };

    private static readonly ConcurrentDictionary<int, Data> DataDictionary = new();

    private readonly IMetadataReferenceService _metadataReferenceService;
    private readonly IProtoFileResolver? _globalProtoFileResolver;

    /// <summary>
    /// Create a new instance of the Converter.
    /// </summary>
    public Converter() : this(new CreateFromFileMetadataReferenceService())
    {
    }

    /// <summary>
    /// Create a new instance of the Converter.
    /// </summary>
    /// <param name="globalProtoFileResolver">Provides an optional virtual file system (used for resolving all .proto files).</param>
    public Converter(IProtoFileResolver? globalProtoFileResolver = null) : this(new CreateFromFileMetadataReferenceService(), globalProtoFileResolver)
    {
    }

    /// <summary>
    /// Create a new instance of the Converter.
    /// </summary>
    /// <param name="metadataReferenceService">The <see cref="IMetadataReferenceService"/> to use.</param>
    /// <param name="globalProtoFileResolver">Provides an optional virtual file system (used for resolving all .proto files).</param>
    public Converter(IMetadataReferenceService metadataReferenceService, IProtoFileResolver? globalProtoFileResolver = null)
    {
        _globalProtoFileResolver = globalProtoFileResolver;
        _metadataReferenceService = Guard.NotNull(metadataReferenceService);
    }

    /// <inheritdoc />
    public async Task<string> ConvertAsync(ConvertToJsonRequest request, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(request);

        var (assembly, inputTypeFullName) = await ParseAsync(request, cancellationToken).ConfigureAwait(false);

        return SerializeUtils.ConvertProtoBufToJson(assembly, inputTypeFullName, request);
    }

    /// <inheritdoc />
    public async Task<object> ConvertAsync(ConvertToObjectRequest request, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(request);

        var (assembly, inputTypeFullName) = await ParseAsync(request, cancellationToken).ConfigureAwait(false);

        return SerializeUtils.ConvertProtoBufToObject(assembly, inputTypeFullName, request.ProtoBufBytes, request.SkipGrpcHeader);
    }

    /// <inheritdoc />
    public async Task<byte[]> ConvertAsync(ConvertToProtoBufRequest request, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(request);

        var (assembly, inputTypeFullName) = await ParseAsync(request, cancellationToken).ConfigureAwait(false);

        var json = request.Input as string ?? SerializeUtils.ConvertObjectToJson(request);

        return SerializeUtils.DeserializeJsonAndConvertToProtoBuf(assembly, inputTypeFullName, json, request.AddGrpcHeader);
    }

    private async Task<(Assembly Assembly, string inputTypeFullName)> ParseAsync(ConvertRequest request, CancellationToken cancellationToken)
    {
        var data = await GetCachedFileDescriptorSetAsync(request.ProtoDefinition, request.ProtoFileResolver, cancellationToken).ConfigureAwait(false);

        var inputTypeFullName = data.Set.GetInputTypeFromMessageType(request.MessageType);

        return (data.Assembly, inputTypeFullName);
    }

    private Task<Data> GetCachedFileDescriptorSetAsync(string protoDefinition, IProtoFileResolver? requestProtoFileResolver, CancellationToken cancellationToken)
    {
        var hashCode = protoDefinition.GetDeterministicHashCode();

        var protoFileResolver = requestProtoFileResolver ?? _globalProtoFileResolver;

        return DataDictionary.GetOrAddAsync(hashCode, key => GetValueAsync(key, protoDefinition, protoFileResolver, cancellationToken));
    }

    private async Task<Data> GetValueAsync(int key, string protoDefinition, IProtoFileResolver? protoFileResolver, CancellationToken cancellationToken)
    {
        var set = new FileDescriptorSet();
        set.Add($"{key}.proto", true, new StringReader(protoDefinition));

        if (protoFileResolver is not null)
        {
            set.FileSystem = new ProtobufJsonConverterFileSystem(protoFileResolver);
        }

        set.AddImportPath(string.Empty);
        set.ApplyFileDependencyOrder();
        set.Process();

        var errors = set.GetErrors();
        if (errors.Any())
        {
            throw new ArgumentException($"Error parsing proto definition. Errors: {string.Join(",", errors.Select(e => e.Message))}", nameof(protoDefinition));
        }

        var code = GenerateCSharpCode(set);

        var data = new Data
        {
            HashCode = key,
            Set = set,
            Assembly = await AssemblyUtils.CompileCodeToAssemblyAsync(code, _metadataReferenceService, cancellationToken).ConfigureAwait(false)
        };

        return data;
    }

    private static string GenerateCSharpCode(FileDescriptorSet set)
    {
        var files = CSharpCodeGenerator.Default.Generate(set, NameNormalizer.Null, CodeGenerateOptions);

        return string.Join(Environment.NewLine, files.Select(f => f.Text));
    }
}