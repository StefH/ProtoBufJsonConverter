using ProtoBuf;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.NullValue", Origin = "google/protobuf/struct.proto")]
public struct NullValue : IWellKnownType
{
    //[ProtoMember(1)]
    //public object? Value
    //{
    //    get => null;
    //    set => throw new InvalidOperationException("NullValue is immutable.");
    //}
}