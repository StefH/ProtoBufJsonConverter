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

    private const string DefaultPrefix = "type.googleapis.com";

    public object? GetUnderlyingValue() => GetUnderlyingValue(TypeUrl, Value);

    public static object? GetUnderlyingValue(string typeUrl, ByteString value)
    {
        if (string.IsNullOrEmpty(typeUrl))
        {
            throw new InvalidOperationException("TypeUrl is null or empty.");
        }

        var fullname = $"ProtoBufJsonConverter.ProtoBuf.WellKnownTypes.{typeUrl.Split('.').LastOrDefault()}";
        var welKnownType = Type.GetType(fullname);
        if (welKnownType == null)
        {
            throw new InvalidOperationException($"Type {fullname} not found.");
        }

        using var memoryStream = ProtoBufUtils.GetMemoryStreamFromBytes(value.ToArray(), true);

        return TryFindGenericType(welKnownType, out var genericType) ? Serializer.Deserialize(genericType, memoryStream) : null;
    }

    private static bool TryFindGenericType(Type type, out Type? genericType)
    {
        var wellKnownTypeInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IWellKnownType<>));

        genericType = wellKnownTypeInterface?.GetGenericArguments().FirstOrDefault();

        return genericType != null;
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