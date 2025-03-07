using SkiaSharp;
using Svg.Skia;
using System.Diagnostics;
using System.Text;

Console.WriteLine("SVG to WebP Converter PoC");
Console.WriteLine("=========================\n");

var sampleDir = Path.Combine(Directory.GetCurrentDirectory(), "samples");
Directory.CreateDirectory(sampleDir);

var sampleSvgPath = Path.Combine(sampleDir, "sample.svg");
if (!File.Exists(sampleSvgPath))
{
    Console.WriteLine("Creating sample SVG file...");
    File.WriteAllText(sampleSvgPath, GenerateSampleSvg());
}

var originalFileInfo = new FileInfo(sampleSvgPath);

Console.WriteLine("\nFile Size Comparison:");
Console.WriteLine($"Original SVG:  {FormatFileSize(originalFileInfo.Length)}");

RunBenchmark(sampleSvgPath, sampleDir);

static void RunBenchmark(string sampleSvgPath, string outputDir)
{
    Console.WriteLine("\n=== BENCHMARKS ===\n");

    var results = new StringBuilder();

    results.AppendLine("| Scale Factor | Quality | Time (ms) | Memory (MB) | File Size | Compression Ratio |");
    results.AppendLine("|--------------|---------|-----------|-------------|-----------|-------------------|");

    var originalFileInfo = new FileInfo(sampleSvgPath);
    var originalSize = originalFileInfo.Length;

    float[] scaleFactors = { 1.0f, 2.0f, 4.0f, 8.0f };
    int[] qualities = { 75, 90, 100 };

    foreach (var scaleFactor in scaleFactors)
    {
        foreach (var quality in qualities)
        {
            var outputPath = Path.Combine(
                outputDir,
                $"sample_scale{scaleFactor}_q{quality}.webp"
            );

            var (conversionTime, memoryUsed, fileSize) = BenchmarkConversion(
                sampleSvgPath,
                outputPath,
                quality,
                quality == 100,
                scaleFactor
            );

            double compressionRatio = (double)originalSize / fileSize;

            results.AppendLine(
                $"| {scaleFactor,12:F1} | {quality,7} | {conversionTime.TotalMilliseconds,9:F2} | " +
                $"{memoryUsed / (1024 * 1024),11:F2} | {FormatFileSize(fileSize),9} | " +
                $"{compressionRatio,17:F2}x |"
            );
        }
    }

    Console.WriteLine(results.ToString());
}

static (TimeSpan conversionTime, long memoryUsed, long fileSize) BenchmarkConversion(
    string svgPath,
    string webpPath,
    int quality = 100,
    bool lossless = true,
    float scaleFactor = 4.0f
)
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
    long startMemory = GC.GetTotalMemory(true);

    var stopwatch = Stopwatch.StartNew();

    ConvertSvgToWebP(svgPath, webpPath, quality, lossless, scaleFactor);

    stopwatch.Stop();

    GC.Collect();
    GC.WaitForPendingFinalizers();

    long endMemory = GC.GetTotalMemory(true);
    long memoryUsed = endMemory - startMemory;

    var fileInfo = new FileInfo(webpPath);
    long fileSize = fileInfo.Length;

    return (stopwatch.Elapsed, memoryUsed, fileSize);
}

static void ConvertSvgToWebP(
    string svgPath,
    string webpPath,
    int quality = 100,
    bool lossless = true,
    float scaleFactor = 4.0f
)
{
    using var svg = new SKSvg();
    if (svg.Load(svgPath) == null)
    {
        throw new Exception($"Failed to load SVG file: {svgPath}");
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

    using var stream = File.OpenWrite(webpPath);
    data.SaveTo(stream);
}

static string FormatFileSize(long bytes)
{
    string[] suffixes = { "B", "KB", "MB", "GB" };
    int counter = 0;
    decimal number = bytes;

    while (Math.Round(number / 1024) >= 1)
    {
        number /= 1024;
        counter++;
    }

    return $"{number:n2} {suffixes[counter]}";
}

static string GenerateSampleSvg()
{
    return @"<svg width=""400"" height=""300"" xmlns=""http://www.w3.org/2000/svg"">
  <!-- Background with gradient -->
  <defs>
    <linearGradient id=""grad1"" x1=""0%"" y1=""0%"" x2=""100%"" y2=""100%"">
      <stop offset=""0%"" style=""stop-color:#f5f7fa;stop-opacity:1"" />
      <stop offset=""100%"" style=""stop-color:#c3cfe2;stop-opacity:1"" />
    </linearGradient>
  </defs>
  <rect width=""400"" height=""300"" fill=""url(#grad1)"" />
  
  <!-- Text elements with different styles -->
  <text x=""50"" y=""50"" font-family=""Arial"" font-size=""24"" font-weight=""bold"" fill=""#333"">
    SVG to WebP Converter
  </text>
  <text x=""50"" y=""80"" font-family=""Arial"" font-size=""14"" fill=""#555"">
    High-quality conversion using SkiaSharp
  </text>
  
  <!-- Some vector shapes -->
  <circle cx=""100"" cy=""150"" r=""40"" stroke=""#3498db"" stroke-width=""3"" fill=""#3498db"" fill-opacity=""0.7"" />
  <rect x=""160"" y=""110"" width=""80"" height=""80"" rx=""10"" ry=""10"" fill=""#e74c3c"" />
  <polygon points=""300,110 340,190 260,190"" fill=""#2ecc71"" />
  
  <!-- A path with curves -->
  <path d=""M 50 220 C 100 180, 150 180, 200 220 S 300 260, 350 220"" stroke=""#9b59b6"" stroke-width=""4"" fill=""transparent"" />
  
  <!-- Some decorative elements -->
  <circle cx=""300"" cy=""70"" r=""15"" fill=""#f1c40f"" />
  <circle cx=""330"" cy=""70"" r=""8"" fill=""#e67e22"" />
  <circle cx=""350"" cy=""70"" r=""5"" fill=""#e74c3c"" />
</svg>";
}
