using Microsoft.AspNetCore.Mvc;
using SvgToWebpApi.Services;
using System.Text;

namespace SvgToWebpApi.Controllers;

[ApiController]
public class ConversionController : ControllerBase
{
    private readonly ImageConversionService _conversionService;
    private readonly ILogger<ConversionController> _logger;

    public ConversionController(
        ImageConversionService conversionService,
        ILogger<ConversionController> logger
    )
    {
        _conversionService = conversionService;
        _logger = logger;
    }

    [HttpPost("api/conversion/svg-to-webp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult ConvertSvgToWebP([FromBody] ConversionRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SvgContent))
            {
                return BadRequest("SVG content is required");
            }

            byte[] svgBytes = Encoding.UTF8.GetBytes(request.SvgContent);

            byte[] webpBytes = _conversionService.ConvertSvgToWebP(
                svgBytes,
                request.Quality ?? 100,
                request.Lossless ?? true,
                request.ScaleFactor ?? 4.0f
            );

            return File(webpBytes, "image/webp");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting SVG to WebP");
            return StatusCode(500, $"Error converting image: {ex.Message}");
        }
    }
}

public class ConversionRequest
{
    public string SvgContent { get; set; } = string.Empty;
    public int? Quality { get; set; }
    public bool? Lossless { get; set; }
    public float? ScaleFactor { get; set; }
}
