## SVG to WebP Converter PoC using [SkiaSharp](https://github.com/mono/SkiaSharp?tab=readme-ov-file)

### Endpoint

`POST api/conversion/svg-to-webp`

**Request Body:**
```json
{
  "svgContent": "<svg>...</svg>",
  "quality": 90,
  "lossless": false,
  "scaleFactor": 4.0
}
```

```bash
curl -X POST "http://localhost:5141/api/conversion/svg-to-webp" \
  -H "Content-Type: application/json" \
  -d "{\"svgContent\":\"<svg width=\\\"100\\\" height=\\\"100\\\"><circle cx=\\\"50\\\" cy=\\\"50\\\" r=\\\"40\\\" fill=\\\"red\\\"/></svg>\", \"quality\": 90, \"lossless\": false, \"scaleFactor\": 4.0}" \
  -o converted.webp
```

### BENCHMARKS

### File Size Comparison:
### Original SVG:  1.31 KB

| Scale Factor | Quality | Time (ms) | File Size | Compression Ratio |
|--------------|---------|-----------|-----------|-------------------|
|          1.0 |      75 |    288.25 |   5.98 KB |             0.22x |
|          1.0 |      90 |     91.83 |   9.67 KB |             0.14x |
|          1.0 |     100 |     83.32 |  26.66 KB |             0.05x |
|          2.0 |      75 |    112.70 |  12.48 KB |             0.11x |
|          2.0 |      90 |    104.32 |  21.12 KB |             0.06x |
|          2.0 |     100 |     96.09 |  35.39 KB |             0.04x |
|          4.0 |      75 |    158.09 |  27.48 KB |             0.05x |
|          4.0 |      90 |    154.98 |  44.49 KB |             0.03x |
|          4.0 |     100 |    139.43 |  71.33 KB |             0.02x |
|          8.0 |      75 |    379.55 |  62.39 KB |             0.02x |
|          8.0 |      90 |    381.34 |  99.30 KB |             0.01x |
|          8.0 |     100 |    281.73 | 143.35 KB |             0.01x |

- **Best Quality:** Scale Factor 4.0, Quality 100 (lossless)
- **Balanced:** Scale Factor 2.0, Quality 90
- **Small File Size:** Scale Factor 1.0, Quality 75
