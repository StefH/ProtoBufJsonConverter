using Google.Protobuf.Reflection;

namespace ProtoBufJsonConverter.IO;

internal interface IDependencyAwareFileSystem : IFileSystem
{
    public List<string> Dependencies { get; }
}