using System.Buffers.Binary;
using System.Reflection;
using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
using ProtoBuf;
using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverter.Utils;

internal static class SerializeUtils
{
    private static readonly Lazy<IJsonConverter> DefaultJsonConverter = new(() => new NewtonsoftJsonConverter());

    internal static string ConvertObjectToJson(ConvertToProtoBufRequest request)
    {
        return (request.JsonConverter ?? DefaultJsonConverter.Value).Serialize(request.Input.Second, request.JsonConverterOptions);
    }

    internal static string ConvertProtoBufToJson(Assembly assembly, string inputTypeFullName, ConvertToJsonRequest request)
    {
        var value = ConvertProtoBufToObject(assembly, inputTypeFullName, request.ProtoBufBytes, request.SkipGrpcHeader);

        return (request.JsonConverter ?? DefaultJsonConverter.Value).Serialize(value, request.JsonConverterOptions);
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
        bool addGrpcHeader,
        IJsonConverter? jsonConverter
    )
    {
        var type = AssemblyUtils.GetType(assembly, inputTypeFullName);

        var instance = (jsonConverter ?? DefaultJsonConverter.Value).Deserialize(json, type);

        return ProtoBufUtils.Serialize((ms) => Serializer.Serialize(ms, instance), addGrpcHeader);
    }
}