using FontStashSharp.Interfaces;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.Fonts;

namespace piconavx.ui.graphics.font
{
    public class SixLaborsFontSource : IFontSource
    {
        private FontGlyphSource _source;
        private FontCollection _fontCollection;
        private float AscentBase, DescentBase, LineHeightBase;

        public SixLaborsFontSource(byte[] data)
        {
            _fontCollection = new FontCollection();
            FontFamily fontInstance;
            using (var ms = new MemoryStream(data))
            {
                fontInstance = _fontCollection.Add(ms);
                _source = new FontGlyphSource(fontInstance);
            }

            if (fontInstance.TryGetMetrics(FontStyle.Regular, out var metrics))
            {
                var fh = metrics.VerticalMetrics.Ascender - metrics.VerticalMetrics.Descender;
                AscentBase = metrics.VerticalMetrics.Ascender / (float)fh;
                DescentBase = metrics.VerticalMetrics.Descender / (float)fh;
                LineHeightBase = metrics.VerticalMetrics.LineHeight / (float)fh;
            } else
            {
                throw new InvalidOperationException("Could not get font metrics");
            }
        }

        public void Dispose()
        {
        }

        public int? GetGlyphId(int codepoint)
        {
            return codepoint;
        }

        public int GetGlyphKernAdvance(int previousGlyphId, int glyphId, float fontSize)
        {
            var kerning = _source.GetKerning(fontSize, previousGlyphId, glyphId);
            return (int)kerning.X;
        }

        public void GetGlyphMetrics(int glyphId, float fontSize, out int advance, out int x0, out int y0, out int x1, out int y1)
        {
            var path = _source.CreatePath(fontSize, glyphId);
            advance = (int)_source.GetAdvance(fontSize, glyphId);
            if (path != null)
            {
                x0 = path.Bounds.X;
                y0 = path.Bounds.Y;
                x1 = path.Bounds.Right;
                y1 = path.Bounds.Bottom;
            } else
            {
                x0 = 0;
                y0 = 0;
                x1 = 0;
                y1 = 0;
            }
        }

        public void GetMetricsForSize(float fontSize, out int ascent, out int descent, out int lineHeight)
        {
            ascent = (int)(fontSize * AscentBase + 0.5f);
            descent = (int)(fontSize * DescentBase - 0.5f);
            lineHeight = (int)(fontSize * LineHeightBase + 0.5f);
        }

        public void RasterizeGlyphBitmap(int glyphId, float fontSize, byte[] buffer, int startIndex, int outWidth, int outHeight, int outStride)
        {
            var path = _source.CreatePath(fontSize, glyphId);

            if (path == null)
                throw new ArgumentNullException("path", "CreatePath returned null");

            Image<Rgba32> image = new Image<Rgba32>(path.Bounds.Width, path.Bounds.Height);
            _source.DrawGlyphToImage(path, new System.Drawing.Point(0, 0), image);

            for (var y = 0; y < outHeight; ++y)
            {
                var pos = (y * outStride) + startIndex;
                for (var x = 0; x < outWidth; ++x)
                {
                    var color = image[x, y];
                    buffer[pos] = color.A;
                    ++pos;
                }
            }
        }
    }
}
