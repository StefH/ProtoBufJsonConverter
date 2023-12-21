using System.Reflection;
using JsonConverter.Abstractions;
using ProtoBuf;

namespace ProtoBufJsonConverter.Utils;

internal static class JsonUtils
{
    public static string Serialize(Assembly assembly, string inputTypeFullName, IJsonConverter jsonConverter, byte[] protoBufBytes)
    {
        var type = GetType(assembly, inputTypeFullName);

        var value = Serializer.Deserialize(type, new MemoryStream(protoBufBytes));

        return jsonConverter.Serialize(value);
    }

    public static byte[] Deserialize(Assembly assembly, string inputTypeFullName, IJsonConverter jsonConverter, string json)
    {
        var type = GetType(assembly, inputTypeFullName);

        var instance = jsonConverter.Deserialize(json, type);

        using var memoryStream = new MemoryStream();
        Serializer.Serialize(memoryStream, instance);

        return memoryStream.ToArray();
    }

    private static Type GetType(Assembly assembly, string inputTypeFullName)
    {
        var type = assembly.GetType(inputTypeFullName);
        if (type == null)
        {
            throw new ArgumentException($"The type {type} cannot be found in the assembly.");
        }

        return type;
    }
}