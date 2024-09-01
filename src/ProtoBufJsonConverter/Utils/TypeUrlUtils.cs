using System.Diagnostics.CodeAnalysis;
using Google.Protobuf.WellKnownTypes;
using ProtoBuf;
using Stef.Validation;

namespace ProtoBufJsonConverter.Utils;

internal static class TypeUrlUtils
{
    private const string TypeGoogleApisComPrefix = "type.googleapis.com";
    private static readonly Lazy<Dictionary<string, Type>> WellKnownTypes = new(InitializeWellKnownTypes);

    internal static bool TryGetUnwrappedValue<T>(string typeUrl, ByteString value, [NotNullWhen(true)] out T? result)
        where T : notnull
    {
        var unwrapped = GetUnwrappedValue(typeUrl, value);
        if (unwrapped == null)
        {
            result = default;
            return false;
        }

        result = (T)unwrapped;
        return true;
    }

    internal static object? GetUnwrappedValue(string typeUrl, ByteString value)
    {
        Guard.NotNull(typeUrl);
        Guard.NotNull(value);

        if (!TryGetTypeFromTypeUrl(typeUrl, out var type))
        {
            throw new InvalidOperationException($"No type found for TypeUrl: {typeUrl}.");
        }

        using var memoryStream = ProtoBufUtils.GetMemoryStreamFromBytes(value.ToArray(), true);
        if (ReflectionUtils.TryFindGenericType(type, out var genericType))
        {
            return Serializer.Deserialize(genericType, memoryStream);
        }

        return Serializer.Deserialize(type, memoryStream);
    }

    internal static string BuildTypeUrl(Type type)
    {
        Guard.NotNull(type);

        if (ReflectionUtils.TryGetProtoContractAttribute(type, out var protoContractAttribute))
        {
            var name = protoContractAttribute.Name;

            if (typeof(IWellKnownType).IsAssignableFrom(type))
            {
                return $"{TypeGoogleApisComPrefix}/{name.TrimStart('.')}";
            }

            if (string.IsNullOrEmpty(name))
            {
                name = type.FullName;
            }

            return $"{TypeGoogleApisComPrefix}/{name!.TrimStart('.')}";
        }

        return $"{TypeGoogleApisComPrefix}/{type.FullName}";
    }

    internal static bool TryGetTypeFromTypeUrl(string typeUrl, [NotNullWhen(true)] out Type? type)
    {
        Guard.NotNull(typeUrl);

        if (typeUrl.StartsWith(TypeGoogleApisComPrefix) && typeUrl.Contains('/'))
        {
            var typeName = typeUrl.Split('/').Last();

            // First try the WellKnownTypes
            if (WellKnownTypes.Value.TryGetValue(typeName, out type))
            {
                return true;
            }

            // Else try to find the type in the generated protobuf assemblies or if not found, search all types.
            if (AssemblyUtils.TryGetType(typeName, out type))
            {
                return true;
            }

            // Try to get the type based on the fully qualified name
            type = Type.GetType(typeName);
            return type != null;
        }

        type = null;
        return false;
    }

    private static Dictionary<string, Type> InitializeWellKnownTypes()
    {
        var types = typeof(IWellKnownType).Assembly
            .GetTypes()
            .Where(type => typeof(IWellKnownType).IsAssignableFrom(type) || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IWellKnownType<>)));

        var wellKnownTypes = new Dictionary<string, Type>();
        foreach (var type in types)
        {
            if (ReflectionUtils.TryGetProtoContractAttribute(type, out var protoContractAttribute))
            {
                wellKnownTypes[protoContractAttribute.Name.TrimStart('.')] = type;
            }
        }

        return wellKnownTypes;
    }


}