using System.Diagnostics.CodeAnalysis;
using ProtoBuf;
using ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;
using Stef.Validation;

namespace ProtoBufJsonConverter.Utils;

internal static class TypeUrlUtils
{
    private const string CustomPrefix = "custom";
    private const string TypeGoogleApisComPrefix = "type.googleapis.com";
    private const string WellKnownTypesNamespace = "ProtoBufJsonConverter.ProtoBuf.WellKnownTypes";

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

    internal static string BuildTypeUrl(object value)
    {
        Guard.NotNull(value);
        var type = value.GetType();

        if (typeof(IWellKnownType).IsAssignableFrom(type))
        {
            return $"{TypeGoogleApisComPrefix}/google.protobuf.{type.Name}";
        }

        return $"{CustomPrefix}/{type.FullName}";
    }

    internal static string GetFindTypeUrl(Type type, string defaultTypeUrl)
    {
        if (type.GetCustomAttributes(typeof(ProtoContractAttribute), false).FirstOrDefault() is ProtoContractAttribute protoContractAttribute)
        {
            var name = protoContractAttribute.Name;
            if (string.IsNullOrEmpty(name))
            {
                name = type.Name;
            }

            if (typeof(IWellKnownType).IsAssignableFrom(type))
            {
                return $"{TypeGoogleApisComPrefix}/{name.TrimStart('.')}";
            }
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