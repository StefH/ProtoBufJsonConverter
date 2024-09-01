using Google.Protobuf.Reflection;

namespace ProtoBufJsonConverter.IO;

internal class ProtobufJsonConverterFileSystem(IProtoFileResolver resolver) : IFileSystem
{
    public bool Exists(string path)
    {
        if (path.StartsWith("google/") || path.StartsWith("protobuf-net/"))
        {
            return false;
        }
        
        return resolver.Exists(path);
    }

    public TextReader OpenText(string path)
    {
        return resolver.OpenText(path);
    }
}