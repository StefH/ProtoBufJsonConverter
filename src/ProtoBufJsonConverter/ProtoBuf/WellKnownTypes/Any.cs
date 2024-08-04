using ProtoBuf;
using ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;
using ProtoBufJsonConverter.Utils;
using Stef.Validation;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.Any", Serializer = typeof(AnySerializer), Origin = "google/protobuf/any.proto")]
public class Any : IWellKnownType
{
    #region Proto
    [ProtoMember(1, IsRequired = true)]
    public string TypeUrl { get; set; } = string.Empty;

    [ProtoMember(2, IsRequired = true)]
    public ByteString Value { get; set; } = [];
    #endregion

    public object? GetUnderlyingValue() => TypeUrlUtils.GetUnderlyingValue(TypeUrl, Value);

    public T Unpack<T>()
    {
        using var ms = new MemoryStream(Value.ToArray());
        return Serializer.Deserialize<T>(ms);
    }
    
    public static Any Pack(object value)
    {
        Guard.NotNull(value);

        return new Any
        {
            TypeUrl = TypeUrlUtils.BuildTypeUrl(value),
            Value = new ByteString(SerializeUtils.Serialize(value))
        };
    }
}