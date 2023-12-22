using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ProtoBuf;

namespace ProtoBufJsonConverter.Utils;

internal static class AssemblyUtils
{
    internal static Assembly CompileCodeToAssembly(string code, CancellationToken cancellationToken)
    {
        // Specify the assembly name
        var assemblyName = Path.GetRandomFileName();

        // Parse the source code
        var syntaxTree = CSharpSyntaxTree.ParseText(code, cancellationToken: cancellationToken);

        // Locate the directory that contains the framework assemblies
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        // Reference the necessary assemblies
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System.Private.CoreLib
            MetadataReference.CreateFromFile(typeof(ProtoContractAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),

            // MetadataReference.CreateFromFile(typeof(CallContext).Assembly.Location), // ProtoBuf.Grpc.CallContext
            // MetadataReference.CreateFromFile(typeof(ServiceContractAttribute).Assembly.Location) // ServiceContract
        };

        // Create a compilation
        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Compile the source code
        using var stream = new MemoryStream();
        var result = compilation.Emit(stream, cancellationToken: cancellationToken);

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