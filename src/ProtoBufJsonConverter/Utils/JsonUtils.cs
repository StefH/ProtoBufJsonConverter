﻿using System.Reflection;
using ProtoBuf;
using ProtoBufJsonConverter.Models;

namespace ProtoBufJsonConverter.Utils;

internal static class JsonUtils
{
    public static string Serialize(Assembly assembly, string inputTypeFullName, ConvertToJsonRequest request)
    {
        var type = GetType(assembly, inputTypeFullName);

        using var memoryStream = new MemoryStream(request.ProtoBufBytes);
        var value = Serializer.Deserialize(type, memoryStream);

        return request.JsonConverter.Serialize(value, request.JsonConverterOptions);
    }

    public static byte[] Deserialize(Assembly assembly, string inputTypeFullName, ConvertToProtoBufRequest request)
    {
        var type = GetType(assembly, inputTypeFullName);

        var instance = request.JsonConverter.Deserialize(request.Json, type);

        using var memoryStream = new MemoryStream();
        Serializer.Serialize(memoryStream, instance);

        return memoryStream.ToArray();
    }

    private static Type GetType(Assembly assembly, string inputTypeFullName)
    {
        var type = assembly.GetType(inputTypeFullName);
        if (type == null)
        {
            throw new ArgumentException($"The type '{type}' cannot be found in the assembly.");
        }

        return type;
    }
}