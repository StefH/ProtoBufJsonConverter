using ProtoBuf;

namespace ProtoBufJsonConverter
{
    [ProtoContract]
    public class StringValue
    {
        [ProtoMember(1)]
        public string Value { get; set; } = string.Empty;
    }
}
