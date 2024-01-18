namespace MetadataReferenceService.BlazorWasm.WasmWebcil.Extensions;

internal static class MathExtensions
{
    //internal static int RoundToNearestV1(this int number, int nearest = 512)
    //{
    //    var divided = 1.0 * number / nearest;
    //    var rounded = (int)Math.Round(divided, MidpointRounding.AwayFromZero);
    //    return rounded * nearest;
    //}

    //internal static uint RoundToNearestV1(this uint number, uint nearest = 512)
    //{
    //    var divided = 1.0 * number / nearest;
    //    var rounded = (uint)Math.Round(divided, MidpointRounding.AwayFromZero);
    //    return rounded * nearest;
    //}

    internal static int RoundToNearest(this int number, int nearest = 512)
    {
        int remainder = number % nearest;
        int halfNearest = nearest / 2;
        return remainder >= halfNearest ? number + nearest - remainder : number - remainder;
    }

    internal static uint RoundToNearest(this uint number, uint nearest = 512)
    {
        uint remainder = number % nearest;
        uint halfNearest = nearest / 2;
        return remainder >= halfNearest ? number + nearest - remainder : number - remainder;
    }
}