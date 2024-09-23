namespace ProtoBufJsonConverter.IO;

internal static class PathChecker
{
    public static bool IncludeFile(string path)
    {
        if (path == null) 
        {
            return false;
        }

        if (path.StartsWith("google/") || path.StartsWith("protobuf-net/"))
        {
            return false;
        }
        
        return true;
    }
}