using ProtoBuf;
using ProtoBuf.Serializers;
using ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

// ReSharper disable once CheckNamespace
namespace Google.Protobuf.WellKnownTypes;

internal class AnySerializer : ISerializer<Any>
{
    SerializerFeatures ISerializer<Any>.Features => SerializerFeatures.WireTypeStartGroup | SerializerFeatures.CategoryMessage;

    Any ISerializer<Any>.Read(ref ProtoReader.State reader, Any any)
    {
        var fh1 = reader.ReadFieldHeader();
        if (fh1 != 1)
        {
            throw new InvalidDataException("Expected Field 1");
        }

        any = new Any
        {
            TypeUrl = reader.ReadString()
        };

        var fh2 = reader.ReadFieldHeader();
        if (fh2 != 2)
        {
            throw new InvalidDataException("Expected Field 2");
        }

        any.Value = reader.ReadAny<ByteString>();

        return any;
    }


    void ISerializer<Any>.Write(ref ProtoWriter.State writer, Any any)
    {
        writer.WriteString(1, any.TypeUrl);

        foreach (var @byte in any.Value)
        {
            writer.WriteFieldHeader(2, WireType.Varint);
            writer.WriteByte(@byte);
        }
    }
}