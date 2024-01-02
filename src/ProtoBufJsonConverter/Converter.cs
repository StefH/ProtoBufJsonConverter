using System.Collections.Concurrent;
using System.Reflection;
using Google.Protobuf.Reflection;
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

    /// <inheritdoc />
    public string Convert(ConvertToJsonRequest request, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(request);

        var (assembly, inputTypeFullName) = Parse(request, cancellationToken);

        return SerializeUtils.ConvertProtoBufToJson(assembly, inputTypeFullName, request);
    }

    /// <inheritdoc />
    public object Convert(ConvertToObjectRequest request, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(request);

        var (assembly, inputTypeFullName) = Parse(request, cancellationToken);

        return SerializeUtils.ConvertProtoBufToObject(assembly, inputTypeFullName, request.ProtoBufBytes, request.SkipGrpcHeader);
    }

    /// <inheritdoc />
    public byte[] Convert(ConvertToProtoBufRequest request, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(request);

        var (assembly, inputTypeFullName) = Parse(request, cancellationToken);

        var json = request.Input as string ?? SerializeUtils.ConvertObjectToJson(request);

        return SerializeUtils.DeserializeJsonAndConvertToProtoBuf(assembly, inputTypeFullName, json, request.AddGrpcHeader, request.JsonConverter);
    }

    private static (Assembly Assembly, string inputTypeFullName) Parse(ConvertRequest request, CancellationToken cancellationToken)
    {
        var data = GetCachedFileDescriptorSet(request.ProtoDefinition, cancellationToken);

        var inputTypeFullName = data.Set.GetInputTypeFromMessageType(request.MessageType);

        return (data.Assembly, inputTypeFullName);
    }

    private static Data GetCachedFileDescriptorSet(string protoDefinition, CancellationToken cancellationToken)
    {
        var protoDefinitionHashCode = protoDefinition.GetDeterministicHashCode();

        return DataDictionary.GetOrAdd(protoDefinitionHashCode, hashCode =>
        {
            var set = new FileDescriptorSet();
            set.Add($"{hashCode}.proto", true, new StringReader(protoDefinition));
            set.Process();

            var errors = set.GetErrors();
            if (errors.Any())
            {
                throw new ArgumentException($"Error parsing proto definition. Errors = {string.Join(",", errors.Select(e => e.Message))}", nameof(protoDefinition));
            }

            var code = GenerateCSharpCode(set);

            return new Data
            {
                HashCode = hashCode,
                Set = set,
                Assembly = AssemblyUtils.CompileCodeToAssembly(code, cancellationToken)
            };
        });
    }

    private static string GenerateCSharpCode(FileDescriptorSet set)
    {
        var files = CSharpCodeGenerator.Default.Generate(set, NameNormalizer.Null, CodeGenerateOptions);

        return string.Join(Environment.NewLine, files.Select(f => f.Text));
    }
}