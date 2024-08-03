using ProtoBuf;
using ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;
using Stef.Validation;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.Any", Serializer = typeof(AnySerializer), Origin = "google/protobuf/any.proto")]
public struct Any
{
    #region Proto
    [ProtoMember(1, IsRequired = true)]
    public string TypeUrl { get; set; }

    [ProtoMember(2, IsRequired = true)] 
    public ByteString Value { get; set; }
    #endregion

    private const string DefaultPrefix = "type.googleapis.com";

    public object? GetUnderlyingValue()
    {
        if (string.IsNullOrEmpty(TypeUrl))
        {
            throw new InvalidOperationException("TypeUrl is null or empty.");
        }

        var fullname = $"ProtoBufJsonConverter.ProtoBuf.WellKnownTypes.{TypeUrl.Split(".").LastOrDefault()}";
        var type = Type.GetType(fullname);
        if (type == null)
        {
            throw new InvalidOperationException($"Type {fullname} not found.");
        }

        using var ms = new MemoryStream(Value.ToArray());
        var value = Serializer.Deserialize(ms, type);
    }

    public static Any Pack(object type)
    {
        Guard.NotNull(type);

        using var ms = new MemoryStream();
        Serializer.Serialize(ms, type);

        return new Any
        {
            TypeUrl = $"{DefaultPrefix}/google.protobuf.{type.GetType().Name}",
            Value = new ByteString(ms.ToArray())
        };
    }

    public T Unpack<T>()
    {
        using var ms = new MemoryStream(Value.ToArray());
        return Serializer.Deserialize<T>(ms);
    }
}