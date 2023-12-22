using System.Reflection;
using ProtoBuf;

namespace ProtoBufJsonConverter.Utils;

internal static class ProtoBufUtils
{
    internal static object Deserialize(Assembly assembly, string inputTypeFullName, byte[] protoBufBytes)
    {
        var type = AssemblyUtils.GetType(assembly, inputTypeFullName);

        using var memoryStream = new MemoryStream(protoBufBytes);
        return Serializer.Deserialize(type, memoryStream);
    }

    internal static byte[] Serialize(string inputTypeFullName, object instance)
    {
        using var memoryStream = new MemoryStream();
        Serializer.Serialize(memoryStream, instance);

        return memoryStream.ToArray();
    }
}