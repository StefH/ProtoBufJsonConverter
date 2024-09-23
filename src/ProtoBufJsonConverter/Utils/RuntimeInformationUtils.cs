using System.Runtime.InteropServices;

namespace ProtoBufJsonConverter.Utils;

internal static class RuntimeInformationUtils
{
    public static bool IsBlazorWASM { get; }

    static RuntimeInformationUtils()
    {
#if NET462
        IsBlazorWASM = false;
#else
        IsBlazorWASM =
            // Used for Blazor WebAssembly .NET Core 3.x / .NET Standard 2.x
            Type.GetType("Mono.Runtime") != null ||

            // Use for Blazor WebAssembly .NET
            // See also https://github.com/mono/mono/pull/19568/files
            RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER")) ||

            // https://github.com/dotnet/aspnetcore/issues/18268
            RuntimeInformation.IsOSPlatform(OSPlatform.Create("WEBASSEMBLY"));
#endif
    }
}