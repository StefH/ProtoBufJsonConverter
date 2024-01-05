using System;
using System.IO;
using System.Text;

namespace Microsoft.NET.WebAssembly.Webcil;

public class WebcilWasmUnwrapper
{
    // Method to extract the Webcil payload from a Wasm-wrapped stream and write it to the provided Stream.
    public void WriteWasmUnwrapped(Stream webcilPayloadStream, Stream wasmWrappedStream)
    {
        if (wasmWrappedStream == null || webcilPayloadStream == null)
            throw new ArgumentNullException("Streams cannot be null.");

        // Ensure the input stream supports seeking, as we'll need to navigate through it.
        if (!wasmWrappedStream.CanSeek)
            throw new ArgumentException("Input stream must support seeking.");

        // Read the header to verify this is a Wasm file.
        byte[] header = new byte[8];
        wasmWrappedStream.Read(header, 0, header.Length);
        if (!IsWasmHeader(header))
            throw new InvalidOperationException("The provided stream does not appear to be in the Wasm format.");

        // Skip until we find the data section, which contains the Webcil payload.
        byte[] buffer = new byte[1];
        while (true)
        {
            wasmWrappedStream.Read(buffer, 0, 1);
            // Check for the Data section (ID = 11)
            if (buffer[0] == 11)
                break;

            // Skip other sections by reading and ignoring their content.
            SkipSection(wasmWrappedStream);
        }

        // Read and ignore the size of the data section.
        ReadULEB128(wasmWrappedStream);

        // Read the number of segments.
        int segmentsCount = (int)ReadULEB128(wasmWrappedStream);
        for (int i = 0; i < segmentsCount; i++)
        {
            // Read and ignore the segment's type.
            wasmWrappedStream.Read(buffer, 0, 1);

            // Read the segment size.
            uint segmentSize = ReadULEB128(wasmWrappedStream);

            // The actual Webcil payload is expected to be in the last segment.
            if (i == segmentsCount - 1)
            {
                // Copy the segment content to the output stream.
                CopyStream(wasmWrappedStream, webcilPayloadStream, segmentSize);
            }
            else
            {
                // Skip other segments.
                wasmWrappedStream.Seek(segmentSize, SeekOrigin.Current);
            }
        }
    }

    private bool IsWasmHeader(byte[] header)
    {
        return true; //Encoding.ASCII.GetString(header) == "\0asm\1\0\0\0";
    }

    private void SkipSection(Stream stream)
    {
        uint size = ReadULEB128(stream);
        stream.Seek(size, SeekOrigin.Current);
    }

    private uint ReadULEB128(Stream stream)
    {
        uint result = 0;
        int shift = 0;
        byte byteValue;
        do
        {
            byteValue = (byte)stream.ReadByte();
            result |= (uint)(byteValue & 0x7F) << shift;
            shift += 7;
        } while ((byteValue & 0x80) != 0);
        return result;
    }

    private void CopyStream(Stream input, Stream output, uint bytesToCopy)
    {
        byte[] buffer = new byte[81920]; // 80 KB buffer.
        int read;
        while (bytesToCopy > 0 &&
               (read = input.Read(buffer, 0, (int)Math.Min(buffer.Length, bytesToCopy))) > 0)
        {
            output.Write(buffer, 0, read);
            bytesToCopy -= (uint)read;
        }
    }
}