using ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Interfaces;

namespace ProtoBufJsonConverter.Extensions;

internal static class TypeExtensions
{
    internal static bool IsWellKnownType(this Type type)
    {
        return typeof(IWellKnownType).IsAssignableFrom(type) || 
               type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IWellKnownType<>));
    }
}