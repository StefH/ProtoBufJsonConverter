using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ProtoBuf;
using ProtoBufJsonConverter.Services;

namespace ProtoBufJsonConverter.Utils;

internal static class AssemblyUtils
{
    // Get and load all required assemblies to compile the SyntaxTree
    private static readonly Lazy<Assembly[]> RequiredAssemblies = new(() =>
    {
        var requiredAssemblies = Assembly.GetEntryAssembly()!
            .GetReferencedAssemblies()
            .Select(Assembly.Load)
            .ToList();
        requiredAssemblies.Add(typeof(object).Assembly);
        requiredAssemblies.Add(typeof(ProtoContractAttribute).Assembly);

        return requiredAssemblies.Distinct().ToArray();
    });

    internal static async Task<Assembly> CompileCodeToAssemblyAsync(string code, IMetadataReferenceService metadataReferenceService, CancellationToken cancellationToken)
    {
        // Specify the assembly name
        var assemblyName = Path.GetRandomFileName();

        // Parse the source code
        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);

        var references = new List<MetadataReference>();
        foreach (var requiredAssembly in RequiredAssemblies.Value)
        {
            references.Add(await metadataReferenceService.CreateAsync(requiredAssembly, cancellationToken).ConfigureAwait(false));
        }

        // Create a compilation
        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
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

    internal static Type GetType(Assembly assembly, string inputTypeFullName)
    {
        var type = assembly.GetType(inputTypeFullName);
        if (type == null)
        {
            throw new ArgumentException($"The type '{type}' cannot be found in the assembly.");
        }

        return type;
    }
}