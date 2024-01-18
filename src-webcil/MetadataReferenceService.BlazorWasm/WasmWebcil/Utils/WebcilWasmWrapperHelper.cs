using System.Reflection;
using Microsoft.NET.WebAssembly.Webcil;

namespace MetadataReferenceService.BlazorWasm.WasmWebcil.Utils;

internal static class WebcilWasmWrapperHelper
{
    public static byte[] GetPrefix()
    {
        var fieldInfo = typeof(WebcilWasmWrapper).GetField("s_wasmWrapperPrefix", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField)!;
#if NET7_0_OR_GREATER
        var readOnlyMemoryByteArray = (ReadOnlyMemory<byte>)fieldInfo.GetValue(null)!;
        return readOnlyMemoryByteArray.ToArray();

#else
        return (byte[])fieldInfo.GetValue(null)!;
#endif
    }
}