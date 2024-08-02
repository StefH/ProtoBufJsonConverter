extern alias gpb;
using System.Buffers.Binary;
using gpb::Google.Protobuf;
using gpb::Google.Protobuf.WellKnownTypes;
using ProtoBuf;
using ProtoBuf.Meta;
using ProtoBuf.Serializers;

namespace ProtoBufJsonConverter.Utils;

/// <summary>
/// Based on https://github.com/pawitp/protobuf-decoder/blob/master/src/protobufDecoder.js
/// </summary>
internal static class ProtoBufUtils
{
    private const int SizeOfUInt32 = 4;
    private const int HeaderSize = 1 + SizeOfUInt32; // 1 (Compression flag) + 4 (UInt32)

    // - https://protobuf.dev/reference/protobuf/google.protobuf/
    internal static readonly (System.Type Type, string[] MemberNames)[] WellKnownTypes =
    [
        (typeof(Any), [nameof(Any.TypeUrl), nameof(Any.Value)]),
        //(typeof(ByteString), []),
        //(typeof(BoolValue), [nameof(BoolValue.Value)]),
        //(typeof(BytesValue), [nameof(BytesValue.Value)]),
        //(typeof(DoubleValue), [nameof(DoubleValue.Value)]),
        //(typeof(FloatValue), [nameof(FloatValue.Value)]),
        //(typeof(Int32Value), [nameof(Int32Value.Value)]),
        //(typeof(Int64Value), [nameof(Int64Value.Value)]),
        //(typeof(StringValue), [nameof(StringValue.Value)]),
        //(typeof(UInt32Value), [nameof(UInt32Value.Value)]),
        //(typeof(UInt64Value), [nameof(UInt64Value.Value)])
    ];

    static ProtoBufUtils()
    {
        // Initialization
        // - https://github.com/protobuf-net/protobuf-net/issues/722

        var typeModel = RuntimeTypeModel.Default;

        typeModel.Add<ByteString>();
        typeModel.AddSerializer(typeof(ByteString), typeof(R));

        //RuntimeTypeModel.Default.Add<ByteString>().SerializerType = typeof(R);

        foreach (var wellKnownType in WellKnownTypes)
        {
            var metaType = typeModel.Add(wellKnownType.Type);

            for (var i = 0; i < wellKnownType.MemberNames.Length; i++)
            {
                metaType.AddField(1 + i, wellKnownType.MemberNames[i]);
            }
        }

        // RuntimeTypeModel.Default.Add<ByteString>().SetSurrogate(typeof(byte[]));

        //typeModel.AutoAddMissingTypes = false;
        //typeModel.SetSurrogate<ByteString, string>(ByteStringToString, StringToByteString);
        //typeModel.SetSurrogate<ByteString, byte[]>(null, BytesToByteString);
        //typeModel.AddSerializer(typeof(ByteString), RepeatedSerializer.CreateEnumerable<ByteString, ByteString, byte>().GetType());
        
        //
        //typeModel.MakeDefault();
    }

    

    class R : IRepeatedSerializer<ByteString>
    {
        public static IRepeatedSerializer<ByteString> Create()
        {
            return new R();
        }

        public SerializerFeatures Features { get; }

        public ByteString Read(ref ProtoReader.State state, ByteString value)
        {
            throw new NotImplementedException();
        }

        public void Write(ref ProtoWriter.State state, ByteString value)
        {
            throw new NotImplementedException();
        }
        public void WriteRepeated(ref ProtoWriter.State state, int fieldNumber, SerializerFeatures features, ByteString values)
        {
            throw new NotImplementedException();
        }

        public ByteString ReadRepeated(ref ProtoReader.State state, SerializerFeatures features, ByteString values)
        {
            throw new NotImplementedException();
        }
    }

    private static string ByteStringToString(ByteString byteString) => byteString.ToStringUtf8();

    private static ByteString StringToByteString(string text) => ByteString.CopyFromUtf8(text);

    private static byte[] ByteStringToBytes(ByteString byteString) => byteString.ToByteArray();

    private static ByteString BytesToByteString(byte[] bytes) => ByteString.CopyFrom(bytes);

    internal static MemoryStream GetMemoryStreamFromBytes(byte[] buffer, bool skipGrpcHeader)
    {
        var offset = 0;

        if (skipGrpcHeader && buffer[offset] == 0 && BytesLeftInBuffer(buffer, offset) >= HeaderSize)
        {
            offset++; // Compression flag

            var length = ReadUInt32BE(buffer, offset);

            offset += SizeOfUInt32; // Message length

            if (length > BytesLeftInBuffer(buffer, offset))
            {
                // Something is wrong, revert
                offset = 0;
            }
        }

        var memoryStream = new MemoryStream(buffer);
        memoryStream.Position = offset;
        return memoryStream;
    }

    internal static byte[] Serialize(Action<MemoryStream> action, bool addGrpcHeader)
    {
        using var memoryStream = new MemoryStream();

        if (addGrpcHeader)
        {
            memoryStream.Position = HeaderSize;
        }

        action(memoryStream);

        var bytes = memoryStream.ToArray();

        if (bytes.Length > 0 && addGrpcHeader)
        {
            var length = bytes.Length - HeaderSize;
            WriteHeader(bytes, length);
        }

        return bytes;
    }

    private static void WriteHeader(Span<byte> headerData, int length)
    {
        // Compression flag
        headerData[0] = 0;

        // Message length
        BinaryPrimitives.WriteUInt32BigEndian(headerData.Slice(1), (uint)length);
    }

    private static int BytesLeftInBuffer(Span<byte> buffer, int offset)
    {
        return buffer.Length - offset;
    }

    private static uint ReadUInt32BE(byte[] buffer, int offset)
    {
        // If the system architecture is little-endian, reverse the byte order
        if (BitConverter.IsLittleEndian)
        {
            return ((uint)buffer[offset] << 24) |
                   ((uint)buffer[offset + 1] << 16) |
                   ((uint)buffer[offset + 2] << 8) |
                   buffer[offset + 3];
        }

        // If the system is already big-endian, read the bytes in order
        return BitConverter.ToUInt32(buffer, offset);
    }

    //class EnumerableSerializer<TCollection, TCreate, T> : RepeatedSerializer<TCollection, T>
    //    where TCollection : class, IEnumerable<T>
    //    where TCreate : TCollection
    //{
    //    protected override int TryGetCount(TCollection values)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override TCollection Clear(TCollection values, ISerializationContext context)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override TCollection AddRange(TCollection values, ref ArraySegment<T> newValues, ISerializationContext context)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override void Measure(TCollection values, IMeasuringSerializer<T> serializer, ISerializationContext context, WireType wireType)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override void WritePacked(ref ProtoWriter.State state, TCollection values, IMeasuringSerializer<T> serializer, WireType wireType)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override void Write(ref ProtoWriter.State state, int fieldNumber, SerializerFeatures features, WireType wireType, TCollection values, ISerializer<T> serializer, SerializerFeatures itemFeatures)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}