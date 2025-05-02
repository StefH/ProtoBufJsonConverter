using System.Runtime.CompilerServices;

namespace ProtoBufJsonConverter.Utils;

internal static class TypeUtils
{
    internal static T ChangeType<T>(object? value, T defaultValue) where T : notnull
    {
        if (value == null)
        {
            return defaultValue;
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }

    internal static bool IsAnonymousType(Type type)
    {
        return type is { IsClass: true, IsSealed: true, Namespace: null, IsNotPublic: true } && 
               type.Name.StartsWith("<>f__AnonymousType", StringComparison.Ordinal) && 
               Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute));
    }
}