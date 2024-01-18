using Microsoft.NET.WebAssembly.Webcil;

namespace MetadataReferenceService.BlazorWasm.WasmWebcil.Extensions;

internal static class WebcilSectionHeaderExtensions
{
    internal static uint GetCorrectedPointerToRawData(this WebcilSectionHeader webcilSectionHeader, int offset)
    {
        return (uint) (webcilSectionHeader.PointerToRawData + offset);
    }
}