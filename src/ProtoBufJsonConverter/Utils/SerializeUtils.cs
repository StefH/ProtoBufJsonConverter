using System.Reflection;
using Newtonsoft.Json;
using ProtoBuf;
using ProtoBufJsonConverter.Converters;
using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverter.Utils;

internal static class SerializeUtils
{
    internal static string ConvertObjectToJson(ConvertToProtoBufRequest request)
    {
        return JsonConvert.SerializeObject(request.Input, GetWellKnownTypesConverter(request));
    }

    internal static string ConvertProtoBufToJson(Assembly assembly, string inputTypeFullName, ConvertToJsonRequest request)
    {
        var value = ConvertProtoBufToObject(assembly, inputTypeFullName, request.ProtoBufBytes, request.SkipGrpcHeader);

        return JsonConvert.SerializeObject(value, new JsonSerializerSettings
        {
            Formatting = request.WriteIndented ? Formatting.Indented : Formatting.None,
            Converters = [GetWellKnownTypesConverter(request)]
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
        ConvertToProtoBufRequest request
    )
    {
        var type = AssemblyUtils.GetType(assembly, inputTypeFullName);

        var instance = JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings
        {
            Converters = [GetWellKnownTypesConverter(request)]
        });

        return Serialize(instance, request.AddGrpcHeader);
    }

    internal static byte[] Serialize(object? instance, bool addGrpcHeader = false)
    {
        return ProtoBufUtils.Serialize(memoryStream => Serializer.Serialize(memoryStream, instance), addGrpcHeader);
    }

    private static WellKnownTypesConverter GetWellKnownTypesConverter(ConvertRequest request)
    {
        return new WellKnownTypesConverter(request.SupportNewerGoogleWellKnownTypes);
    }
}