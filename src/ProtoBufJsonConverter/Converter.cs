using System.Collections.Concurrent;
using System.Reflection;
using Google.Protobuf.Reflection;
using ProtoBuf.Reflection;
using ProtoBufJsonConverter.Extensions;
using ProtoBufJsonConverter.Models;
using ProtoBufJsonConverter.Utils;
using Stef.Validation;

namespace ProtoBufJsonConverter;

public class Converter : IConverter
{
    private static readonly Dictionary<string, string> CodeGenerateOptions = new()
    {
        { "nullablevaluetype", "true" }
    };

    private static readonly ConcurrentDictionary<int, Data> DataDictionary = new();

    public string ConvertToJson(ConvertToJsonRequest request, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(request);

        var (assembly, inputTypeFullName) = Parse(request.ProtoDefinition, request.Method, cancellationToken);

        return JsonUtils.Serialize(assembly, inputTypeFullName, request.JsonConverter, request.ProtobufBytes);
    }

    public byte[] ConvertToProtoBuf(ConvertToProtoBufRequest request, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(request);

        var (assembly, inputTypeFullName) = Parse(request.ProtoDefinition, request.Method, cancellationToken);

        return JsonUtils.Deserialize(assembly, inputTypeFullName, request.JsonConverter, request.Json);
    }

    private static (Assembly Assembly, string inputTypeFullName) Parse(string protoDefinition, string method, CancellationToken cancellationToken)
    {
        var data = GetCachedFileDescriptorSet(protoDefinition, cancellationToken);

        var inputTypeFullName = GetInputType(data.Set, method);

        return (data.Assembly, inputTypeFullName);
    }

    private static Data GetCachedFileDescriptorSet(string protoDefinition, CancellationToken cancellationToken)
    {
        var hashcode = protoDefinition.GetDeterministicHashCode();

        return DataDictionary.GetOrAdd(hashcode, hc =>
        {
            var set = new FileDescriptorSet();
            set.Add($"{hc}.proto", true, new StringReader(protoDefinition));
            set.Process();

            var errors = set.GetErrors();
            if (errors.Any())
            {
                throw new ArgumentException($"Error parsing proto definition. Errors = {string.Join(",", errors.Select(e => e.Message))}", nameof(protoDefinition));
            }

            var code = GenerateCSharpCode(set);

            return new Data
            {
                HashCode = hc,
                Set = set,
                Assembly = AssemblyUtils.CompileCodeToAssembly(code, cancellationToken)
            };
        });
    }

    private static string GetInputType(FileDescriptorSet set, string method)
    {
        var parts = method.Split('.');

        string packageName = string.Empty;
        string serviceName;
        string methodName;
        switch (parts.Length)
        {
            case 3:
                packageName = parts[0];
                serviceName = parts[1];
                methodName = parts[2];
                break;

            case 2:
                serviceName = parts[0];
                methodName = parts[1];
                break;

            default:
                throw new ArgumentException($"The method '{method}' is not valid.");
        }

        var fileDescriptorProto = set.Files.FirstOrDefault(f => f.Package == packageName);
        if (fileDescriptorProto == null)
        {
            throw new ArgumentException($"The package '{packageName}' is not found in the proto definition.");
        }


        var serviceDescriptorProto = fileDescriptorProto.Services.FirstOrDefault(s => s.Name == serviceName);
        if (serviceDescriptorProto == null)
        {
            throw new ArgumentException($"The service '{serviceName}' is not found in the proto definition.");
        }

        var methodDescriptorProto = serviceDescriptorProto.Methods.FirstOrDefault(m => m.Name == methodName);
        if (methodDescriptorProto == null)
        {
            throw new ArgumentException($"The method '{methodName}' is not found in the proto definition.");
        }

        return methodDescriptorProto.InputType.Trim('.');
    }

    private static string GenerateCSharpCode(FileDescriptorSet set)
    {
        var files = CSharpCodeGenerator.Default.Generate(set, NameNormalizer.Null, CodeGenerateOptions);

        return string.Join(Environment.NewLine, files.Select(f => f.Text));
    }
}