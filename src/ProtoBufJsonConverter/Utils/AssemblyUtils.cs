﻿using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MetadataReferenceService.Abstractions;
using MetadataReferenceService.Abstractions.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ProtoBuf;
using ProtoBuf.WellKnownTypes;

namespace ProtoBufJsonConverter.Utils;

internal static class AssemblyUtils
{
    // Get and load all required assemblies to compile the SyntaxTree
    private static readonly Lazy<IReadOnlyList<Assembly>> RequiredAssemblies = new(() =>
    {
        var assemblySystemRuntime = Assembly.GetEntryAssembly()!
            .GetReferencedAssemblies()
            .First(a => a.Name == "System.Runtime");

        return new List<Assembly>
        {
            Assembly.Load(assemblySystemRuntime),
            typeof(object).Assembly,
            typeof(ProtoContractAttribute).Assembly,
            typeof(AssemblyUtils).Assembly
        };
    });
    private static readonly ConcurrentDictionary<string, Type> ExtraTypesFromCompiledCode = new();
    private static readonly ConcurrentDictionary<string, Type> ExtraTypesFromAllAssemblies = new();

    internal static async Task<Assembly> CompileCodeToAssemblyAsync(string code, IMetadataReferenceService metadataReferenceService, CancellationToken cancellationToken)
    {
        // Specify the assembly name
        var assemblyName = Path.GetRandomFileName();

        // Parse the source code
        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);

        // Create a compilation
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [syntaxTree],
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

    internal static bool TryGetType(string name, [NotNullWhen(true)] out Type? type)
    {
        if (ExtraTypesFromCompiledCode.TryGetValue(name, out type))
        {
            return true;
        }

        // Last resort, search all loaded assemblies for the type
        var allTypesWithProtoContractAttribute = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.GetCustomAttribute<ProtoContractAttribute>(false) != null)
            .ToArray();

        type = allTypesWithProtoContractAttribute.FirstOrDefault(t => t.FullName == name || t.Name == name);
        if (type != null)
        {
            Add(ExtraTypesFromAllAssemblies, type, name);
            return true;
        }

        return false;
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

    internal static Type GetType(Assembly assembly, string inputTypeFullName)
    {
        return inputTypeFullName switch
        {
            "google.protobuf.Empty" => typeof(Empty),
            "google.protobuf.Duration" => typeof(Duration),
            "google.protobuf.Timestamp" => typeof(Timestamp),
            _ => assembly.GetType(inputTypeFullName) ?? throw new ArgumentException($"The type '{inputTypeFullName}' is not found in the assembly.")
        };
    }
}