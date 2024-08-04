using System.Diagnostics.CodeAnalysis;
using Google.Protobuf.WellKnownTypes;
using ProtoBuf;
using Stef.Validation;

namespace ProtoBufJsonConverter.Utils;

internal static class TypeUrlUtils
{
    private const string CustomPrefix = "custom";
    private const string TypeGoogleApisComPrefix = "type.googleapis.com";
    private const string WellKnownTypesNamespace = "Google.Protobuf.WellKnownTypes";

    internal static object? GetUnderlyingValue(string typeUrl, ByteString value)
    {
        Guard.NotNull(typeUrl);
        Guard.NotNull(value);

        using var memoryStream = ProtoBufUtils.GetMemoryStreamFromBytes(value.ToArray(), true);

        if (!TryGetTypeFromTypeUrl(typeUrl, out var welKnownType))
        {
            throw new InvalidOperationException($"No type found for TypeUrl: {typeUrl}.");
        }

        return ReflectionUtils.TryFindGenericType(welKnownType, out var genericType) ? Serializer.Deserialize(genericType, memoryStream) : null;
    }

    internal static string BuildTypeUrl(Type type)
    {
        Guard.NotNull(type);

        if (type.GetCustomAttributes(typeof(ProtoContractAttribute), false).FirstOrDefault() is ProtoContractAttribute protoContractAttribute)
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

            return $"{CustomPrefix}/{name!.TrimStart('.')}";
        }

        return $"{CustomPrefix}/{type.FullName}";
    }

    internal static bool TryGetTypeFromTypeUrl(string typeUrl, [NotNullWhen(true)] out Type? type)
    {
        Guard.NotNull(typeUrl);

        type = null;

        if (typeUrl.StartsWith(CustomPrefix))
        {
            type = Type.GetType(typeUrl.Substring(CustomPrefix.Length + 1));
            return type != null;
        }

        if (typeUrl.StartsWith(TypeGoogleApisComPrefix))
        {
            var typeName = $"{WellKnownTypesNamespace}.{typeUrl.Split('.').Last()}";
            type = Type.GetType(typeName);
            return type != null;
        }

        return false;
    }
}