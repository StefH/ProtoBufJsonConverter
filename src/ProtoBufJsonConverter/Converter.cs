using System.Collections.Concurrent;
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
    /// <param name="globalProtoFileResolver">Provides a virtual file system (used for resolving all .proto files).</param>
    public Converter(IProtoFileResolver globalProtoFileResolver) : this(new CreateFromFileMetadataReferenceService(), globalProtoFileResolver)
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

        return SerializeUtils.DeserializeJsonAndConvertToProtoBuf(assembly, inputTypeFullName, json, request);
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

        return DataDictionary.GetOrAddAsync(hashCode, key => GetDataAsync(key, protoDefinition, protoFileResolver, cancellationToken));
    }

    private async Task<Data> GetDataAsync(int key, string protoDefinition, IProtoFileResolver? protoFileResolver, CancellationToken cancellationToken)
    {
        var set = new FileDescriptorSet();
        set.Add($"{key}.proto", true, new StringReader(protoDefinition));

        IDependencyAwareFileSystem dependencyAwareFileSystem;
        if (protoFileResolver != null)
        {
            dependencyAwareFileSystem = new ProtobufJsonConverterFileSystem(protoFileResolver);
        }
        else
        {
            dependencyAwareFileSystem = new DefaultDependencyAwareFileSystem();
        }
        set.FileSystem = dependencyAwareFileSystem;
        set.AddImportPath(string.Empty);
        set.ApplyFileDependencyOrder();
        set.Process();

        var errorsAndWarnings = set.GetErrors();
        var errors = errorsAndWarnings.Where(e => e.IsError).ToArray();
        if (errors.Any())
        {
            throw new ArgumentException($"Error parsing proto definition. Error(s): {string.Join(",", errors.Select(e => e.Message))}", nameof(protoDefinition));
        }

        // For each file that is a dependency, include it in the output c# code file
        foreach (var file in set.Files.Where(file => dependencyAwareFileSystem.Dependencies.Contains(file.Name)))
        {
            file.IncludeInOutput = true;
        }

        var codes = GenerateCSharpCodes(set);

        var data = new Data
        {
            HashCode = key,
            Set = set,
            Assembly = await AssemblyUtils.CompileCodeToAssemblyAsync(codes, _metadataReferenceService, cancellationToken).ConfigureAwait(false)
        };

        return data;
    }

    private static string[] GenerateCSharpCodes(FileDescriptorSet set)
    {
        var files = CSharpCodeGenerator.Default.Generate(set, NameNormalizer.Null, CodeGenerateOptions);

        return files.Select(f => f.Text).ToArray();
    }
}