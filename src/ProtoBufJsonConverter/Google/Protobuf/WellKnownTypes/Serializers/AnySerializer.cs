using ProtoBuf;
using ProtoBuf.Serializers;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes.Serializers;

internal class AnySerializer : ISerializer<Any>
{
    SerializerFeatures ISerializer<Any>.Features => SerializerFeatures.WireTypeStartGroup | SerializerFeatures.CategoryMessage;

    Any ISerializer<Any>.Read(ref ProtoReader.State state, Any any)
    {
        var fh1 = state.ReadFieldHeader();
        if (fh1 != 1)
        {
            throw new InvalidDataException("Expected Field 1");
        }

        any = new Any
        {
            TypeUrl = state.ReadString()
        };

        var fh2 = state.ReadFieldHeader();
        if (fh2 != 2)
        {
            throw new InvalidDataException("Expected Field 2");
        }

        any.Value = state.ReadAny<ByteString>();

        return any;
    }

    void ISerializer<Any>.Write(ref ProtoWriter.State state, Any any)
    {
        state.WriteString(1, any.TypeUrl);

        foreach (var @byte in any.Value)
        {
            state.WriteFieldHeader(2, WireType.Varint);
            state.WriteByte(@byte);
        }
    }
}