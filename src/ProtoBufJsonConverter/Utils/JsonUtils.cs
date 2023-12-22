using System.Reflection;
using JsonConverter.Abstractions;
using JsonConverter.Newtonsoft.Json;
using ProtoBuf;
using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverter.Utils;

internal static class JsonUtils
{
    private static readonly Lazy<IJsonConverter> DefaultJsonConverter = new(() => new NewtonsoftJsonConverter());

    internal static string Serialize(Assembly assembly, string inputTypeFullName, ConvertToJsonRequest request)
    {
        var value = ProtoBufUtils.Deserialize(assembly, inputTypeFullName, request.ProtoBufBytes);

        return (request.JsonConverter ?? DefaultJsonConverter.Value).Serialize(value, request.JsonConverterOptions);
    }

    internal static byte[] Deserialize(Assembly assembly, string inputTypeFullName, ConvertToProtoBufRequest request)
    {
        var type = AssemblyUtils.GetType(assembly, inputTypeFullName);

        var instance = (request.JsonConverter ?? DefaultJsonConverter.Value).Deserialize(request.Json, type);

        using var memoryStream = new MemoryStream();
        Serializer.Serialize(memoryStream, instance);

        return memoryStream.ToArray();
    }
}