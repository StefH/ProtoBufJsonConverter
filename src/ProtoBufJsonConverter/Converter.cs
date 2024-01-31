using System.Collections.Concurrent;
using System.Reflection;
using Google.Protobuf.Reflection;
using MetadataReferenceService.Abstractions;
using MetadataReferenceService.Default;
using ProtoBuf.Reflection;
using ProtoBufJsonConverter.Extensions;
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

    public Converter() : this(new CreateFromFileMetadataReferenceService())
    {
    }

    public Converter(IMetadataReferenceService metadataReferenceService)
    {
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

        return SerializeUtils.DeserializeJsonAndConvertToProtoBuf(assembly, inputTypeFullName, json, request.AddGrpcHeader, request.JsonConverter);
    }

    private async Task<(Assembly Assembly, string inputTypeFullName)> ParseAsync(ConvertRequest request, CancellationToken cancellationToken)
    {
        var data = await GetCachedFileDescriptorSetAsync(request.ProtoDefinition, cancellationToken).ConfigureAwait(false);

        var inputTypeFullName = data.Set.GetInputTypeFromMessageType(request.MessageType);

        return (data.Assembly, inputTypeFullName);
    }

    private async Task<Data> GetCachedFileDescriptorSetAsync(string protoDefinition, CancellationToken cancellationToken)
    {
        var protoDefinitionHashCode = protoDefinition.GetDeterministicHashCode();

        if (DataDictionary.TryGetValue(protoDefinitionHashCode, out var data))
        {
            return data;
        }
        
        var set = new FileDescriptorSet();
        set.Add($"{protoDefinitionHashCode}.proto", true, new StringReader(protoDefinition));
        set.Process();

        var errors = set.GetErrors();
        if (errors.Any())
        {
            throw new ArgumentException($"Error parsing proto definition. Errors = {string.Join(",", errors.Select(e => e.Message))}", nameof(protoDefinition));
        }

        var code = GenerateCSharpCode(set);

        data = new Data
        {
            HashCode = protoDefinitionHashCode,
            Set = set,
            Assembly = await AssemblyUtils.CompileCodeToAssemblyAsync(code, _metadataReferenceService, cancellationToken).ConfigureAwait(false)
        };
        DataDictionary[protoDefinitionHashCode] = data;
        return data;
    }

    private static string GenerateCSharpCode(FileDescriptorSet set)
    {
        var files = CSharpCodeGenerator.Default.Generate(set, NameNormalizer.Null, CodeGenerateOptions);

        return string.Join(Environment.NewLine, files.Select(f => f.Text));
    }
}