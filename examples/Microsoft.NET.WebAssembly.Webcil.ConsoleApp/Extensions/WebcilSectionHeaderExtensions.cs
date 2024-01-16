namespace Microsoft.NET.WebAssembly.Webcil.ConsoleApp.Extensions;

internal static class WebcilSectionHeaderExtensions
{
    internal static uint GetCorrectedPointerToRawData(this WebcilSectionHeader webcilSectionHeader, int offset)
    {
        return (uint) (webcilSectionHeader.PointerToRawData + offset);
    }
}