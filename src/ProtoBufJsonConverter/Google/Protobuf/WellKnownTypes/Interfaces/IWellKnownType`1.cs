namespace ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Interfaces;

public interface IWellKnownType<T> : IWellKnownType
{
    public T Value { get; set; }
}