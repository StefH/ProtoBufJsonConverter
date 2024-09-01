using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
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
public class Converter(IMetadataReferenceService metadataReferenceService) : IConverter
{
    private static readonly Dictionary<string, string> CodeGenerateOptions = new()
    {
        { "nullablevaluetype", "true" }
    };

    private static readonly ConcurrentDictionary<int, Data> DataDictionary = new();

    private readonly IMetadataReferenceService _metadataReferenceService = Guard.NotNull(metadataReferenceService);

    public Converter() : this(new CreateFromFileMetadataReferenceService())
    {
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
        var data = await GetCachedFileDescriptorSetAsync(request.ProtoDefinition, cancellationToken).ConfigureAwait(false);

        var inputTypeFullName = data.Set.GetInputTypeFromMessageType(request.MessageType);

        return (data.Assembly, inputTypeFullName);
    }

    private Task<Data> GetCachedFileDescriptorSetAsync(string protoDefinition, CancellationToken cancellationToken)
    {
        var protoDefinitionsHashCode = protoDefinition.GetDeterministicHashCode();

        return DataDictionary.GetOrAddAsync(protoDefinitionsHashCode, key => GetValueAsync(key, protoDefinition, cancellationToken));
    }

    private async Task<Data> GetValueAsync(int key, string protoDefinition, CancellationToken cancellationToken)
    {
        var set = new FileDescriptorSet();
        set.Add($"{key}.proto", true, new StringReader(protoDefinition));
        set.AddImportPath(string.Empty);
        set.ApplyFileDependencyOrder();
        set.Process();
        
        var errors = set.GetErrors();
        if (errors.Any())
        {
            throw new ArgumentException($"Error parsing proto definition. Errors = {string.Join(",", errors.Select(e => e.Message))}", nameof(protoDefinition));
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

    private bool TryGet(string protoDefinition, [NotNullWhen(true)] out FileDescriptorSet? set)
    {
        var protoDefinitionHashCode = protoDefinition.GetDeterministicHashCode();

        set = new FileDescriptorSet();
        set.Add($"{protoDefinitionHashCode}.proto", true, new StringReader(protoDefinition));
        set.Process();

        var errors = set.GetErrors();
        return !errors.Any();
    }

    private static string GenerateCSharpCode(FileDescriptorSet set)
    {
        var files = CSharpCodeGenerator.Default.Generate(set, NameNormalizer.Null, CodeGenerateOptions);

        return string.Join(Environment.NewLine, files.Select(f => f.Text));
    }
}