using MetadataReferenceService.BlazorWasm.WasmWebcil.Utils;

namespace MetadataReferenceService.BlazorWasm.WasmWebcil;

internal class WasmWebcilUnwrapper : IDisposable
{
    private readonly Stream _wasmStream;

    public WasmWebcilUnwrapper(Stream wasmStream)
    {
        _wasmStream = wasmStream;
    }

    public void WriteUnwrapped(Stream outputStream)
    {
        ValidateWasmPrefix();

        using var reader = new BinaryReader(_wasmStream, System.Text.Encoding.UTF8, leaveOpen: true);
        var bytes = ReadDataSection(reader);
        outputStream.Write(bytes);
    }

    private void ValidateWasmPrefix()
    {
        // Create a byte array matching the length of the prefix.
        var prefix = WebcilWasmWrapperUtils.GetPrefix();
        var buffer = new byte[prefix.Length];
        int bytesRead = _wasmStream.Read(buffer, 0, buffer.Length);
        if (bytesRead < buffer.Length)
        {
            throw new InvalidOperationException("TODO");
        }

        // Compare the read prefix with the expected one.
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] != prefix[i])
            {
                throw new InvalidOperationException("Invalid Wasm prefix.");
            }
        }
    }

    private static void SkipSection(BinaryReader reader)
    {
        var size = ULEB128Decode(reader);
        reader.BaseStream.Seek(size, SeekOrigin.Current);
    }

    private static byte[] ReadDataSection(BinaryReader reader)
    {
        // Skip until we find the data section, which contains the Webcil payload.
        byte[] buffer = new byte[1];
        while (true)
        {
            var dataRead = reader.Read(buffer, 0, 1);
            if (dataRead == 0)
            {
                throw new InvalidOperationException();
            }

            // Check for the Data section (ID = 11)
            if (buffer[0] == 11)
            {
                break;
            }

            // Skip other sections by reading and ignoring their content.
            SkipSection(reader);
        }

        // Read and ignore the size of the data section.
        ULEB128Decode(reader);

        // Read the number of segments.
        int segmentsCount = (int)ULEB128Decode(reader);
        for (int i = 0; i < segmentsCount; i++)
        {
            // Ignore segmentType (1 = passive segment)
            var segmentType = reader.Read(buffer, 0, 1);
            if (segmentType != 1)
            {
                throw new InvalidOperationException($"Unexpected segment code for segment {i}.");
            }

            // Read the segment size.
            uint segmentSize = ULEB128Decode(reader);

            // The actual Webcil payload is expected to be in the last segment.
            if (i == segmentsCount - 1)
            {
                return reader.ReadBytes((int)segmentSize);
            }

            // Skip other segments.
            reader.BaseStream.Seek(segmentSize, SeekOrigin.Current);
        }

        throw new Exception("Unable to read DataSection.");

        /*
        // Read the section code and ensure it's the expected value (11 for Data)
        byte sectionCode = reader.ReadByte();
        if (sectionCode != 11)
            throw new InvalidOperationException("Expected data section not found.");

        // Read and ignore the size of the data section.
        ULEB128Decode(reader);

        // Read the number of segments.
        int segmentsCount = (int)ULEB128Decode(reader);
        for (int i = 0; i < segmentsCount; i++)
        {
            // Read segment 0
            byte segment0Code = reader.ReadByte(); // should be 1 for passive segment
            if (segment0Code != 1)
            {
                throw new InvalidOperationException("Unexpected segment code for segment 0.");
            }

            // Read the segment size.
            uint segmentSize = ULEB128Decode(reader);

            // The actual Webcil payload is expected to be in the last segment.
            if (i == segmentsCount - 1)
            {
                uint webcilPayloadSize = reader.ReadUInt32(); // read the payload size directly as a 32-bit unsigned int
                byte[] webcilPayload = reader.ReadBytes((int)webcilPayloadSize);
                return webcilPayload;
                // Copy the segment content to the output stream.
                //CopyStream(wasmWrappedStream, webcilPayloadStream, segmentSize);
            }
            //else
            //{
            //    // Skip other segments.
            //    //wasmWrappedStream.Seek(segmentSize, SeekOrigin.Current);
            //}

            
        }

        throw new Exception("todo");*/

        /*
        // Read the data section size (ULEB128 encoded)
        uint dataSectionSize = ULEB128Decode(reader);

        // Read the number of segments (expected to be 2)
        byte numSegments = reader.ReadByte();
        if (numSegments != 2)
            throw new InvalidOperationException("Unexpected number of segments.");

        // Read segment 0
        byte segment0Code = reader.ReadByte(); // should be 1 for passive segment
        if (segment0Code != 1)
            throw new InvalidOperationException("Unexpected segment code for segment 0.");

        uint segment0Size = reader.ReadByte(); // this is ULEB128 encoded if > 127, else it's the size.
        if (segment0Size > 127)
            segment0Size = ULEB128Decode(reader);

        uint webcilPayloadSize = reader.ReadUInt32(); // read the payload size directly as a 32-bit unsigned int

        // Skip the padding for segment 0
        reader.BaseStream.Seek(segment0Size - 4, SeekOrigin.Current);

        // Read segment 1
        byte segment1Code = reader.ReadByte(); // should be 1 for passive segment
        if (segment1Code != 1)
        {
            throw new InvalidOperationException("Unexpected segment code for segment 1.");
        }

        uint segment1Size = ULEB128Decode(reader); // ULEB128 encoded size

        // Check alignment
        if (reader.BaseStream.Position % WebcilWasmWrapper.WebcilPayloadInternalAlignment != 0)
        {
            throw new Exception($"Unexpected alignment at position {reader.BaseStream.Position}.");
        }

        // Read the webcil payload
        byte[] webcilPayload = reader.ReadBytes((int)webcilPayloadSize);

        // Here you would typically reconstruct the object or state from the read data.
        return webcilPayload;*/
    }

    private static uint ULEB128Decode(BinaryReader reader)
    {
        uint result = 0;
        int shift = 0;
        byte byteValue;

        do
        {
            byteValue = reader.ReadByte();
            uint byteAsUInt = byteValue & 0x7Fu;
            result |= byteAsUInt << shift;
            shift += 7;
        } while ((byteValue & 0x80) != 0);

        return result;
    }

    //private void ExtractAndWriteWebcilPayload(Stream outputStream)
    //{
    //    // Skipping directly to the payload assuming the structure is exactly as the wrapper defines.
    //    // This might not be robust for all Wasm files, especially if they don't match the structure exactly.

    //    // Skip until where the payload should start.
    //    _wasmStream.Seek(WebcilWasmWrapper.s_wasmWrapperPrefix.Length, SeekOrigin.Begin);

    //    // Read the section header and size.
    //    int sectionId = _wasmStream.ReadByte();
    //    if (sectionId != 11)  // 0x0B is the data section ID
    //    {
    //        throw new InvalidOperationException("Data section not found.");
    //    }

    //    // Read the ULEB128 encoded size of the section.
    //    uint size = ReadULEB128(_wasmStream);

    //    // Skip the rest of the headers and padding to get to the payload.
    //    _wasmStream.Seek(WebcilWasmWrapper.SegmentCodeSize + WebcilWasmWrapper.WebcilPayloadInternalAlignment - 1, SeekOrigin.Current);

    //    // Copy the payload to the output stream.
    //    _wasmStream.CopyTo(outputStream);
    //}

    //private static uint ReadULEB128(Stream stream)
    //{
    //    uint result = 0;
    //    int shift = 0;
    //    byte byteVal;

    //    do
    //    {
    //        byteVal = (byte)stream.ReadByte();
    //        result |= (uint)(byteVal & 0x7F) << shift;
    //        shift += 7;
    //    } while ((byteVal & 0x80) != 0);

    //    return result;
    //}
    public void Dispose()
    {
        _wasmStream.Dispose();
    }
}