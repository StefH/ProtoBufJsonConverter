namespace ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

public interface IWellKnownType<T>
    where T : notnull
{
    public T Value { get; set; }
}