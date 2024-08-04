// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

public interface IWellKnownType<T> : IWellKnownType
{
    public T Value { get; set; }
}