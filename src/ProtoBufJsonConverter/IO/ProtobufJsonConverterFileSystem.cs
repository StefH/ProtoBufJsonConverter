namespace ProtoBufJsonConverter.IO;

internal class ProtobufJsonConverterFileSystem(IProtoFileResolver resolver) : IDependencyAwareFileSystem
{
    public List<string> Dependencies { get; } = [];

    public bool Exists(string path)
    {
        return PathChecker.IncludeFile(path) && resolver.Exists(path);
    }

    public TextReader OpenText(string path)
    {
        Dependencies.Add(path);
        return resolver.OpenText(path);
    }
}