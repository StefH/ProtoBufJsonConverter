using System.Reflection;
using Google.Protobuf.Reflection;

namespace ProtoBufJsonConverter.Models;

internal struct Data
{
    public int HashCode { get; set; }

    public FileDescriptorSet Set { get; set; }

    public Assembly Assembly { get; set; }
}