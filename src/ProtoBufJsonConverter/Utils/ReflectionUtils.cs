using ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

namespace ProtoBufJsonConverter.Utils;

internal static class ReflectionUtils
{
    internal static bool TryFindGenericType(Type type, out Type? genericType)
    {
        var wellKnownTypeInterface = type
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IWellKnownType<>));

        genericType = wellKnownTypeInterface?
            .GetGenericArguments()
            .FirstOrDefault();

        return genericType != null;
    }
}