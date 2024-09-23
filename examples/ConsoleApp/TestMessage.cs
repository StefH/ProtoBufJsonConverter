using System.ComponentModel;
using ProtoBuf;

namespace ConsoleApp;

public class TestMessage : IExtensible
{
    private IExtension _extensionData = null!;

    IExtension IExtensible.GetExtensionObject(bool createIfMissing) => Extensible.GetExtensionObject(ref _extensionData, createIfMissing);

    [ProtoMember(1, Name = "message")]
    [DefaultValue("")]
    public string Message { get; set; } = "";
}