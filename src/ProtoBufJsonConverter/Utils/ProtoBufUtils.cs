using Stef.Validation;

namespace ProtoBufJsonConverter.Utils;

/// <summary>
/// Based on https://github.com/pawitp/protobuf-decoder/blob/master/src/protobufDecoder.js
/// </summary>
internal class ProtoBufUtils
{
    private const int SizeOfInt32 = 4;

    private readonly byte[] _buffer;
    private int _offset;

    internal ProtoBufUtils(byte[] buffer)
    {
        _buffer = Guard.NotNull(buffer);
    }

    internal MemoryStream GetMemoryStream(bool skipGrpcHeader)
    {
        var backupOffset = _offset;

        if (skipGrpcHeader && _buffer[_offset] == 0 && BytesLeftInBuffer() >= 1 + SizeOfInt32)
        {
            _offset++;
            var length = ReadInt32BE(_buffer, _offset);
            _offset += SizeOfInt32;

            if (length > BytesLeftInBuffer())
            {
                // Something is wrong, revert
                _offset = backupOffset;
            }
        }

        var memoryStream = new MemoryStream(_buffer);
        memoryStream.Position = _offset;
        return memoryStream;
    }

    internal int BytesLeftInBuffer()
    {
        return _buffer.Length - _offset;
    }

    private static int ReadInt32BE(byte[] buffer, int offset)
    {
        // If the system architecture is little-endian, reverse the byte order
        if (BitConverter.IsLittleEndian)
        {
            return (buffer[offset] << 24) |
                   (buffer[offset + 1] << 16) |
                   (buffer[offset + 2] << 8) |
                   (buffer[offset + 3]);
        }

        // If the system is already big-endian, read the bytes in order
        return BitConverter.ToInt32(buffer, offset);
    }
}