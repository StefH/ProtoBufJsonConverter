﻿using ProtoBuf;

namespace ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.ByteString", Origin = "google/protobuf/wrappers.proto")]
public class ByteString : List<byte>
{
    public ByteString()
    {
    }

    public ByteString(IEnumerable<byte> bytes)
    {
        AddRange(bytes);
    }
}