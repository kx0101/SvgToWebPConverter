using SkiaSharp;
using Svg.Skia;

namespace SvgToWebpApi.Services;

public class ImageConversionService
{
    public byte[] ConvertSvgToWebP(byte[] svgData, int quality = 100, bool lossless = true, float scaleFactor = 4.0f)
    {
        using var svg = new SKSvg();
        using var svgStream = new MemoryStream(svgData);

        if (svg.Load(svgStream) == null)
        {
            throw new Exception("Failed to load SVG data");
        }

        SKRect bounds = svg.Picture?.CullRect ?? SKRect.Empty;
        if (bounds.IsEmpty)
        {
            throw new Exception("SVG has no dimensions");
        }

        int width = (int)Math.Ceiling(bounds.Width * scaleFactor);
        int height = (int)Math.Ceiling(bounds.Height * scaleFactor);

        width = Math.Max(width, 400);
        height = Math.Max(height, 400);

        var imageInfo = new SKImageInfo(
            width,
            height,
            SKColorType.Rgba8888,
            SKAlphaType.Premul
        );

        using var surface = SKSurface.Create(imageInfo);
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.Transparent);
        canvas.DrawColor(SKColors.Transparent);
        canvas.Scale(scaleFactor);

        var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            SubpixelText = true
        };

        canvas.DrawPicture(svg.Picture, paint: paint);

        using var image = surface.Snapshot();

        int actualQuality = lossless ? 100 : quality;
        using var data = image.Encode(SKEncodedImageFormat.Webp, actualQuality);

        return data.ToArray();
    }
}
