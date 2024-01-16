using System.Reflection;

namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp.Utils;

internal static class WebcilWasmWrapperUtils
{
    public static byte[] GetPrefix()
    {
        var fieldInfo = typeof(WebcilWasmWrapper).GetField("s_wasmWrapperPrefix", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField)!;
        return (byte[])fieldInfo.GetValue(null)!;
    }
}