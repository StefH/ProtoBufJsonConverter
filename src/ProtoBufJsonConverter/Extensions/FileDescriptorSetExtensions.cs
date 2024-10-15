﻿// ReSharper disable once CheckNamespace
namespace Google.Protobuf.Reflection;

internal static class FileDescriptorSetExtensions
{
    internal static string GetInputTypeFromMessageType(this FileDescriptorSet set, string messageType)
    {
        string packageName;
        string typeName;

        var lastDotIndex = messageType.LastIndexOf('.');
        if (lastDotIndex == -1)
        {
            packageName = string.Empty;
            typeName = messageType;
        }
        else
        {
            packageName = messageType.Substring(0, lastDotIndex);
            typeName = messageType.Substring(lastDotIndex + 1);
        }

        var fileDescriptors = set.Files.Where(f => f.Package == packageName).ToArray();
        if (fileDescriptors.Length == 0)
        {
            throw new ArgumentException($"The package '{packageName}' is not found in the proto definition(s).");
        }

        var descriptorProto = fileDescriptors
            .SelectMany(f => f.MessageTypes)
            .FirstOrDefault(proto => proto.Name == typeName);
        if (descriptorProto == null)
        {
            throw new ArgumentException($"The message type '{typeName}' is not found in the proto definition(s).");
        }

        return messageType;
    }

    internal static string GetInputTypeFromServiceMethod(this FileDescriptorSet set, string method)
    {
        var parts = method.Split('.');

        string packageName;
        string serviceName;
        string methodName;
        switch (parts.Length)
        {
            case 3:
                packageName = parts[0];
                serviceName = parts[1];
                methodName = parts[2];
                break;

            case 2:
                packageName = string.Empty;
                serviceName = parts[0];
                methodName = parts[1];
                break;

            default:
                throw new ArgumentException($"The method '{method}' is not valid.");
        }

        var fileDescriptorProto = set.Files.Find(f => f.Package == packageName);
        if (fileDescriptorProto == null)
        {
            throw new ArgumentException($"The package '{packageName}' is not found in the proto definition.");
        }

        var serviceDescriptorProto = fileDescriptorProto.Services.Find(s => s.Name == serviceName);
        if (serviceDescriptorProto == null)
        {
            throw new ArgumentException($"The service '{serviceName}' is not found in the proto definition.");
        }

        var methodDescriptorProto = serviceDescriptorProto.Methods.Find(m => m.Name == methodName);
        if (methodDescriptorProto == null)
        {
            throw new ArgumentException($"The method '{methodName}' is not found in the proto definition.");
        }

        return methodDescriptorProto.InputType.TrimStart('.');
    }
}