using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MetadataReferenceService.Abstractions;
using MetadataReferenceService.Abstractions.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ProtoBuf;
using ProtoBuf.WellKnownTypes;
using ProtoBufJsonConverter.Extensions;
using ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes;

namespace ProtoBufJsonConverter.Utils;

internal static class AssemblyUtils
{
    // Get and load all required assemblies to compile the SyntaxTree
    private static readonly Lazy<IReadOnlyList<Assembly>> RequiredAssemblies = new(() =>
    {
        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => !a.IsDynamic && (RuntimeInformationUtils.IsBlazorWASM || !string.IsNullOrEmpty(a.Location)))
            .ToArray();

        return
        [
            assemblies.First(a => a.GetName().Name == "System.Runtime"),
            assemblies.First(a => a.GetName().Name == "netstandard"),
            typeof(object).Assembly,
            typeof(ProtoContractAttribute).Assembly,
            typeof(AssemblyUtils).Assembly
        ];
    });

    private static readonly Lazy<ConcurrentDictionary<string, Type>> AllTypesWithProtoContractAttribute = new(() =>
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var allTypes = new List<Type>();
        foreach (var assembly in assemblies)
        {
            try
            {
                allTypes.AddRange(assembly.GetTypes());
            }
            catch (ReflectionTypeLoadException ex)
            {
                allTypes.AddRange(ex.Types.OfType<Type>());
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        var allTypesWithAttribute = allTypes
            .Select(t => new
            {
                Type = t,
                ProtoContractAttribute = t.TryGetCustomAttribute<ProtoContractAttribute>(out var attribute) ? attribute : null
            })
            .Where(t => t.ProtoContractAttribute != null)
            .ToArray();

        var dict = new ConcurrentDictionary<string, Type>();
        foreach (var t in allTypesWithAttribute)
        {
            Add(dict, t.Type, t.ProtoContractAttribute!.Name);
        }

        return dict;
    });

    private static readonly ConcurrentDictionary<string, Type> ExtraTypesFromCompiledCode = new();

    internal static async Task<Assembly> CompileCodeToAssemblyAsync(string[] codes, IMetadataReferenceService metadataReferenceService, CancellationToken cancellationToken)
    {
        // Specify the assembly name
        var assemblyName = Path.GetRandomFileName();

        // Parse the source codes
        var syntaxTrees = codes.Select(code => CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken));

        // Create a compilation
        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            await CreateMetadataReferencesAsync(metadataReferenceService, cancellationToken).ConfigureAwait(false),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Compile the source code
        using var stream = new MemoryStream();

        // Wrap this in a task to avoid Blazor Startup Error: System.Threading.SynchronizationLockException: Cannot wait on monitors on this runtime
        var result = await Task.Run(() => compilation.Emit(stream, cancellationToken: cancellationToken), cancellationToken);

        if (!result.Success)
        {
            var failures = result
                .Diagnostics
                .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

            throw new InvalidOperationException($"Unable to compile the code. Errors: {string.Join(",", failures.Select(f => $"{f.Id}-{f.GetMessage()}"))}");
        }

        // Load assembly
        var assembly = Assembly.Load(stream.ToArray());
        foreach (var type in assembly.GetTypes())
        {
            if (!ReflectionUtils.TryGetProtoContractAttribute(type, out var protoContractAttribute))
            {
                continue;
            }

            Add(ExtraTypesFromCompiledCode, type, protoContractAttribute.Name);
        }

        return assembly;
    }

    internal static bool TryGetType(string name, [NotNullWhen(true)] out Type? type)
    {
        if (TryGetGoogleWellKnownType(name, out type))
        {
            return true;
        }

        if (ExtraTypesFromCompiledCode.TryGetValue(name, out type))
        {
            return true;
        }

        // Last resort, search all loaded assemblies for the type
        if (AllTypesWithProtoContractAttribute.Value.TryGetValue(name, out type))
        {
            return true;
        }

        return false;
    }

    internal static bool TryGetType(Assembly assembly, string inputTypeFullName, [NotNullWhen(true)] out Type? type)
    {
        if (TryGetGoogleWellKnownType(inputTypeFullName, out type))
        {
            return true;
        }

        type = assembly.GetType(inputTypeFullName);
        return type != null;
    }

    internal static Type GetType(Assembly assembly, string inputTypeFullName)
    {
        return TryGetType(assembly, inputTypeFullName, out var type) ? type : throw new ArgumentException($"The type '{inputTypeFullName}' is not found in the assembly.");
    }

    internal static bool TryGetGoogleWellKnownType(string inputTypeFullName, [NotNullWhen(true)] out Type? type)
    {
        type = inputTypeFullName switch
        {
            "Google.Protobuf.WellKnownTypes.Any" => typeof(Any),
            "Google.Protobuf.WellKnownTypes.BoolValue" => typeof(BoolValue),
            "Google.Protobuf.WellKnownTypes.ByteString" => typeof(ByteString),
            "Google.Protobuf.WellKnownTypes.BytesValue" => typeof(BytesValue),
            "Google.Protobuf.WellKnownTypes.DoubleValue" => typeof(DoubleValue),
            "Google.Protobuf.WellKnownTypes.Duration" => typeof(Duration),
            "Google.Protobuf.WellKnownTypes.Empty" => typeof(Empty),
            "Google.Protobuf.WellKnownTypes.FloatValue" => typeof(FloatValue),
            "Google.Protobuf.WellKnownTypes.Int32Value" => typeof(Int32Value),
            "Google.Protobuf.WellKnownTypes.Int64Value" => typeof(Int64Value),
            "Google.Protobuf.WellKnownTypes.ListValue" => typeof(ListValue),
            "Google.Protobuf.WellKnownTypes.NullValue" => typeof(NullValue),
            "Google.Protobuf.WellKnownTypes.StringValue" => typeof(StringValue),
            "Google.Protobuf.WellKnownTypes.Struct" => typeof(Struct),
            "Google.Protobuf.WellKnownTypes.Timestamp" => typeof(Timestamp),
            "Google.Protobuf.WellKnownTypes.UInt32Value" => typeof(UInt32Value),
            "Google.Protobuf.WellKnownTypes.UInt64Value" => typeof(UInt64Value),
            "Google.Protobuf.WellKnownTypes.Value" => typeof(Value),

            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Any" => typeof(Any),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.BoolValue" => typeof(BoolValue),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.ByteString" => typeof(ByteString),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.BytesValue" => typeof(BytesValue),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.DoubleValue" => typeof(DoubleValue),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Duration" => typeof(Duration),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Empty" => typeof(Empty),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.FloatValue" => typeof(FloatValue),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Int32Value" => typeof(Int32Value),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Int64Value" => typeof(Int64Value),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.ListValue" => typeof(ListValue),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.NullValue" => typeof(NullValue),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.StringValue" => typeof(StringValue),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Struct" => typeof(Struct),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Timestamp" => typeof(Timestamp),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.UInt32Value" => typeof(UInt32Value),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.UInt64Value" => typeof(UInt64Value),
            "ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Value" => typeof(Value),

            "google.protobuf.Any" => typeof(Any),
            "google.protobuf.BoolValue" => typeof(BoolValue),
            "google.protobuf.ByteString" => typeof(ByteString),
            "google.protobuf.BytesValue" => typeof(BytesValue),
            "google.protobuf.DoubleValue" => typeof(DoubleValue),
            "google.protobuf.Duration" => typeof(Duration),
            "google.protobuf.Empty" => typeof(Empty),
            "google.protobuf.FloatValue" => typeof(FloatValue),
            "google.protobuf.Int32Value" => typeof(Int32Value),
            "google.protobuf.Int64Value" => typeof(Int64Value),
            "google.protobuf.ListValue" => typeof(ListValue),
            "google.protobuf.NullValue" => typeof(NullValue),
            "google.protobuf.StringValue" => typeof(StringValue),
            "google.protobuf.Struct" => typeof(Struct),
            "google.protobuf.Timestamp" => typeof(Timestamp),
            "google.protobuf.UInt32Value" => typeof(UInt32Value),
            "google.protobuf.UInt64Value" => typeof(UInt64Value),
            "google.protobuf.Value" => typeof(Value),

            _ => null
        };

        return type != null;
    }

    private static void Add(ConcurrentDictionary<string, Type> dict, Type type, string? name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            dict.TryAdd(name.TrimStart('.'), type);
        }
        else
        {
            dict.TryAdd(type.FullName!, type);
        }
    }

    private static async Task<IReadOnlyList<MetadataReference>> CreateMetadataReferencesAsync(IMetadataReferenceService metadataReferenceService, CancellationToken cancellationToken)
    {
        var requiredAssemblies = RequiredAssemblies.Value.ToList();
        var references = new List<MetadataReference>();
        foreach (var requiredAssembly in requiredAssemblies)
        {
            references.Add(await metadataReferenceService.CreateAsync(AssemblyDetails.FromAssembly(requiredAssembly), cancellationToken).ConfigureAwait(false));
        }

        return references;
    }
}