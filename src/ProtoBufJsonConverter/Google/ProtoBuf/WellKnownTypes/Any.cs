using ProtoBuf;
using ProtoBufJsonConverter.Utils;
using Stef.Validation;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

/// <summary>
/// This Any class contains an arbitrary serialized protocol buffer message along with a URL that describes the type of the serialized message.
/// </summary>
[ProtoContract(Name = ".google.protobuf.Any", Serializer = typeof(AnySerializer), Origin = "google/protobuf/any.proto")]
public class Any : IWellKnownType
{
    internal const string TypeUrlPropertyName = "@type";
    internal const string ValuePropertyName = "value";

    #region Proto
    [ProtoMember(1, IsRequired = true)]
    public string TypeUrl { get; set; } = string.Empty;

    [ProtoMember(2, IsRequired = true)]
    public ByteString Value { get; set; } = [];
    #endregion

    /// <summary>
    /// Gets the real / unwrapped value
    /// </summary>
    /// <returns>The real/unwrapped value as an object.</returns>
    public object? GetUnwrappedValue() => TypeUrlUtils.GetUnwrappedValue(TypeUrl, Value);

    /// <summary>
    /// Unpacks the content of this Any message into the target message type, which must match the type URL within this Any message.
    /// </summary>
    /// <typeparam name="T">The type of message to unpack the content into.</typeparam>
    /// <returns>The unpacked message.</returns>
    public T Unpack<T>()
    {
        using var ms = new MemoryStream(Value.ToArray());
        return Serializer.Deserialize<T>(ms);
    }

    /// <summary>
    /// Packs the specified message into an Any message using a type URL prefix of "type.googleapis.com".
    /// </summary>
    /// <param name="message">The message to pack.</param>
    /// <returns>An Any message with the content and type URL of message.</returns>
    public static Any Pack(object message)
    {
        Guard.NotNull(message);

        return new Any
        {
            TypeUrl = TypeUrlUtils.BuildTypeUrl(message.GetType()),
            Value = new ByteString(SerializeUtils.Serialize(message))
        };
    }
}