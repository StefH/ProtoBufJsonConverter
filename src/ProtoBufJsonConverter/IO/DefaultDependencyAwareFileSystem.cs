namespace ProtoBufJsonConverter.IO;

internal class DefaultDependencyAwareFileSystem : IDependencyAwareFileSystem
{
    public List<string> Dependencies { get; } = [];

    public bool Exists(string path)
    {
        return PathChecker.IncludeFile(path) && File.Exists(path);
    }

    public TextReader OpenText(string path)
    {
        Dependencies.Add(path);
        return File.OpenText(path);
    }
}