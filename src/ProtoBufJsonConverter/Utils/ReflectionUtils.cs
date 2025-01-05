using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Google.Protobuf.WellKnownTypes.Interfaces;
using ProtoBuf;

namespace ProtoBufJsonConverter.Utils;

internal static class ReflectionUtils
{
    internal static bool TryFindGenericType(Type type, [NotNullWhen(true)] out Type? genericType)
    {
        var wellKnownTypeInterface = type
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IWellKnownType<>));

        genericType = wellKnownTypeInterface?
            .GetGenericArguments()
            .FirstOrDefault();

        return genericType != null;
    }

    internal static bool TryGetProtoContractAttribute(Type type, [NotNullWhen(true)] out ProtoContractAttribute? protoContractAttribute)
    {
        protoContractAttribute = type.GetCustomAttribute<ProtoContractAttribute>(false);
        return protoContractAttribute != null;
    }
}