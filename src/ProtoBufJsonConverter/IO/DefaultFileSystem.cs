using Google.Protobuf.Reflection;

namespace ProtoBufJsonConverter.IO;

internal class DefaultFileSystem : IDependencyAwareFileSystem
{
    public List<string> Dependencies { get; } = [];

    public bool Exists(string path)
    {
        if (path.StartsWith("google/") || path.StartsWith("protobuf-net/"))
        {
            return false;
        }
        
        return File.Exists(path);
    }

    public TextReader OpenText(string path)
    {
        Dependencies.Add(path);
        return File.OpenText(path);
    }
}