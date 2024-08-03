﻿using ProtoBuf;

namespace ProtoBufJsonConverter.ProtoBuf.WellKnownTypes;

[ProtoContract(Name = ".google.protobuf.FloatValue", Origin = "google/protobuf/wrappers.proto")]
public struct FloatValue
{
    [ProtoMember(1)]
    public float Value { get; set; }
}