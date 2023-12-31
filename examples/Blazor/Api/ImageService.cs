using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using Shared;

namespace Api;

internal class ImageService
{
    private static readonly Dictionary<ProcessType, Lazy<MagickImage>> Mapping = new()
    {
        { ProcessType.BlendMStack, new Lazy<MagickImage>(() => new MagickImage(File.ReadAllBytes("Images\\mstack_template_blog - toplaag.png"))) },
        { ProcessType.ProfielMStack, new Lazy<MagickImage>(() => new MagickImage(File.ReadAllBytes("Images\\Kaders_ProfielfotoLinkedIn_mstack.png"))) },
        { ProcessType.ProfielHaystaq, new Lazy<MagickImage>(() => new MagickImage(File.ReadAllBytes("Images\\Kaders_ProfielfotoLinkedIn_haystaq.png"))) },
        { ProcessType.ProfielGroup9, new Lazy<MagickImage>(() => new MagickImage(File.ReadAllBytes("Images\\Kaders_ProfielfotoLinkedIn_group9.png"))) },
        { ProcessType.ProfielAlfa1, new Lazy<MagickImage>(() => new MagickImage(File.ReadAllBytes("Images\\Kaders_ProfielfotoLinkedIn_alfa1.png"))) },
        { ProcessType.ProfielAdaptif, new Lazy<MagickImage>(() => new MagickImage(File.ReadAllBytes("Images\\Kaders_ProfielfotoLinkedIn_adaptif.png"))) },
        { ProcessType.ProfielBluPoint, new Lazy<MagickImage>(() => new MagickImage(File.ReadAllBytes("Images\\Kaders_ProfielfotoLinkedIn_blupoint.png"))) },
        { ProcessType.ProfielNabled, new Lazy<MagickImage>(() => new MagickImage(File.ReadAllBytes("Images\\Kaders_ProfielfotoLinkedIn_n-abled.png"))) }
    };

    public ImageDetails GetDetails(byte[] imageBytes)
    {
        using var sourceImage = new MagickImage(imageBytes);

        return new ImageDetails
        {
            Width = sourceImage.Width,
            Height = sourceImage.Height
        };
    }

    public async Task<byte[]> CombineAsync(CombineRequest request, CancellationToken cancellationToken)
    {
        using var sourceImage = new MagickImage(request.ImageBytes);

        if (!Mapping.TryGetValue(request.ProcessType, out var blendImageLazy))
        {
            throw new NotSupportedException();
        }

        using var blendImage = new MagickImage(blendImageLazy.Value);

        if (sourceImage.Width != blendImage.Width || sourceImage.Height != blendImage.Height || request.ResizeOption == ResizeOption.Custom)
        {
            Resize(blendImage, sourceImage, request.ResizeOption, request.Width ?? -1);
        }

        using var memoryStream = new MemoryStream();

        using var result = Blend(sourceImage, blendImage);

        await result.WriteAsync(memoryStream, MagickFormat.Jpg, cancellationToken).ConfigureAwait(false);

        return memoryStream.ToArray();
    }

    private IMagickImage<ushort> Blend(MagickImage sourceImage, MagickImage blend)
    {
        var images = new MagickImageCollection
        {
            sourceImage,
            blend
        };

        return images.Merge();
    }

    /// <summary>
    /// https://github.com/dlemstra/Magick.NET/blob/main/docs/ResizeImage.md
    ///
    /// This will resize the image to a fixed size without maintaining the aspect ratio.
    /// Normally an image will be resized to fit inside the specified size.
    /// </summary>
    private static void Resize(IMagickImage sourceImage, IMagickImage layerImage, ResizeOption resizeOption, int width)
    {
        switch (resizeOption)
        {
            case ResizeOption.UseLayer:
                layerImage.Resize(GetMagickGeometry(sourceImage.Width));
                break;

            case ResizeOption.UseSource:
                sourceImage.Resize(GetMagickGeometry(layerImage.Width));
                break;

            default:
                sourceImage.Resize(GetMagickGeometry(width));
                layerImage.Resize(GetMagickGeometry(width));
                break;
        }
    }

    private static MagickGeometry GetMagickGeometry(int width)
    {
        return new MagickGeometry(width, width)
        {
            IgnoreAspectRatio = true
        };
    }
}