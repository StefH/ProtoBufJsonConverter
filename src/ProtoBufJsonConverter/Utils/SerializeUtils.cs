using System.Reflection;
using Newtonsoft.Json;
using ProtoBuf;
using ProtoBufJsonConverter.Json;
using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverter.Utils;

internal static class SerializeUtils
{
    private static readonly WellKnownTypesConverter ProtoMessageConverter = new();

    internal static string ConvertObjectToJson(ConvertToProtoBufRequest request)
    {
        return JsonConvert.SerializeObject(request.Input, ProtoMessageConverter);
    }

    internal static string ConvertProtoBufToJson(Assembly assembly, string inputTypeFullName, ConvertToJsonRequest request)
    {
        var value = ConvertProtoBufToObject(assembly, inputTypeFullName, request.ProtoBufBytes, request.SkipGrpcHeader);

        return JsonConvert.SerializeObject(value, new JsonSerializerSettings
        {
            Formatting = request.WriteIndented ? Formatting.Indented : Formatting.None,
            Converters = [ProtoMessageConverter]
        });
    }

    internal static object ConvertProtoBufToObject(Assembly assembly, string inputTypeFullName, byte[] protoBufBytes, bool skipGrpcHeader)
    {
        var type = AssemblyUtils.GetType(assembly, inputTypeFullName);

        using var memoryStream = ProtoBufUtils.GetMemoryStreamFromBytes(protoBufBytes, skipGrpcHeader);

        return Serializer.Deserialize(type, memoryStream);
    }

    internal static byte[] DeserializeJsonAndConvertToProtoBuf(
        Assembly assembly,
        string inputTypeFullName,
        string json,
        bool addGrpcHeader
    )
    {
        var type = AssemblyUtils.GetType(assembly, inputTypeFullName);

        var instance = JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings
        {
            Converters = [ProtoMessageConverter]
        });

        return Serialize(instance, addGrpcHeader);
    }

    internal static byte[] Serialize(object? instance, bool addGrpcHeader = false)
    {
        return ProtoBufUtils.Serialize(memoryStream =>
        {
            Serializer.Serialize(memoryStream, instance);
        }, addGrpcHeader);
    }
}