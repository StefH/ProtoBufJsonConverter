using ProtoBuf;

namespace ProtoBufJsonConverter
{
    [ProtoContract]
    public class StringValue
    {
        [ProtoMember(1)]
        public string value { get; set; } = string.Empty;
    }
}