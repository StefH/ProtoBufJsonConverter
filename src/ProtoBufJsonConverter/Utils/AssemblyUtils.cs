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
        return Assembly.Load(stream.ToArray());
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