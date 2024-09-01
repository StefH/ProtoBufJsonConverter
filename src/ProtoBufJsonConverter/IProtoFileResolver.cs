namespace ProtoBufJsonConverter;

/// <summary>
/// Represents a resolver for .proto files.
/// </summary>
public interface IProtoFileResolver
{
    /// <summary>
    /// Indicates whether a specified file exists.
    /// </summary>
    bool Exists(string path);

    /// <summary>
    /// Opens the specified file for text parsing.
    /// </summary>
    TextReader OpenText(string path);
}