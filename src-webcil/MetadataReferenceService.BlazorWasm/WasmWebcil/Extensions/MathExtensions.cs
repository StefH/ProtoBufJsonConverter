namespace MetadataReferenceService.BlazorWasm.WasmWebcil.Extensions;

internal static class MathExtensions
{
    internal static int RoundToNearest(this int number, int nearest = 512)
    {
        var divided = 1.0 * number / nearest;
        var rounded = (int)Math.Round(divided, MidpointRounding.AwayFromZero);
        return rounded * nearest;
    }

    internal static uint RoundToNearest(this uint number, uint nearest = 512)
    {
        var divided = 1.0 * number / nearest;
        var rounded = (uint)Math.Round(divided, MidpointRounding.AwayFromZero);
        return rounded * nearest;
    }
}