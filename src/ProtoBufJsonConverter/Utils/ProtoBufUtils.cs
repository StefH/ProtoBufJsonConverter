using System.Buffers.Binary;

namespace ProtoBufJsonConverter.Utils;

/// <summary>
/// Some code based on https://github.com/pawitp/protobuf-decoder/blob/master/src/protobufDecoder.js
/// </summary>
internal static class ProtoBufUtils
{
    private const int SizeOfUInt32 = 4;
    private const int HeaderSize = 1 + SizeOfUInt32; // 1 (Compression flag) + 4 (UInt32)
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

    private static int BytesLeftInBuffer(ReadOnlySpan<byte> buffer, int offset)
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
}