namespace ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

public interface IWellKnownType<T> : IWellKnownType
    where T : notnull
{
    public T Value { get; set; }
}

public interface IWellKnownType;