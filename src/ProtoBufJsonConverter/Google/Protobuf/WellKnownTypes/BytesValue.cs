﻿using ProtoBuf;
using ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes.Interfaces;

namespace ProtoBufJsonConverter.Google.Protobuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.BytesValue", Origin = "google/protobuf/wrappers.proto")]
public struct BytesValue : IWellKnownType<byte[]>
{
    [ProtoMember(1, IsRequired = true)]
    public byte[] Value { get; set; }
}