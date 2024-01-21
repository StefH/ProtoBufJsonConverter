using System.Reflection;

namespace MetadataReferenceService.Abstractions.Types;

/// <summary>
/// AssemblyDetails
/// </summary>
public readonly struct AssemblyDetails
{
    /// <summary>
    /// The simple name of the assembly.
    /// This is usually, but not necessarily, the file name of the manifest file of the assembly, minus its extension.
    /// </summary>
    public required string Name { get; init; }


    /// <summary>
    /// The full path or UNC location of the loaded file that contains the manifest. [optional]
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Create a <see cref="AssemblyDetails"/> from an <see cref="Assembly"/>.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns><see cref="AssemblyDetails"/></returns>
    public static AssemblyDetails FromAssembly(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        return new AssemblyDetails
        {
            Name = assembly.GetName().Name!,
            Location = assembly.Location
        };
    }
}