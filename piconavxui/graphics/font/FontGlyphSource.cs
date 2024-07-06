using System;
using System.Collections.Generic;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.Fonts.Unicode;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace piconavx.ui.graphics.font
{
    /// <summary>
	/// An implementation of <see cref="IGlyphSource"/> that sources it's glyphs from
	/// a <see cref="SixLabors.Fonts"/> font.
	/// </summary>
	internal sealed class FontGlyphSource
    {
        private const float Dpi = 96;
        private const float PointsPerInch = 72;

        /// <summary>The <see cref="Font"/> from which this <see cref="FontGlyphSource"/> gets glyph data.</summary>
        public readonly FontFamily FontInstance;

        /// <summary>Configuration for how glyphs should be rendered.</summary>
        public DrawingOptions DrawingOptions;

        /// <summary>Whether to include kerning if present in the font. Default is true.</summary>
        public bool IncludeKerningIfPresent = true;

        /// <summary>
        /// Creates a <see cref="FontGlyphSource"/> instance.
        /// </summary>
        public FontGlyphSource(FontFamily fontInstance)
        {
            FontInstance = fontInstance;

            DrawingOptions = new DrawingOptions
            {
                ShapeOptions = { IntersectionRule = IntersectionRule.NonZero },
            };
        }

        /// <summary>
        /// Creates the <see cref="IPathCollection"/> for all the characters, also getting their colors,
        /// glyph sizes and render offsets.
        /// </summary>
        public GlyphPath? CreatePath(float size, int codepoint)
        {
            var pointSize = size * PointsPerInch / Dpi;
            IPathCollection? p = TextBuilder.GenerateGlyphs(new string((char)codepoint, 1), new TextOptions(FontInstance.CreateFont(pointSize))
            {
                Dpi = Dpi,
                Origin = new Vector2(0,0),
                ColorFontSupport = ColorFontSupport.MicrosoftColrFormat
            });
            RectangleF bounds = p.Bounds;

            var area = bounds.Width * bounds.Height;
            if (area == 0)
            {
                return null;
            }

            if (char.IsWhiteSpace((char)codepoint))
            {
                p = null;
            }

            return new GlyphPath
            {
                Size = size,
                Codepoint = codepoint,
                Bounds = new Rectangle((int)bounds.X, (int)bounds.Y, (int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height)),
                Paths = p
            };
        }

        public float GetAdvance(float size, int codepoint)
        {
            Font font = FontInstance.CreateFont(size);
            if (font.TryGetGlyphs(new CodePoint(codepoint), out var glyphs))
            {
                if(glyphs.Count > 0)
                {
                    return glyphs[0].GlyphMetrics.AdvanceWidth / font.FontMetrics.UnitsPerEm;
                }
            }
            throw new Exception("Invalid glyph list");
        }

        public Vector2 GetKerning(float size, int codepoint1, int codepoint2)
        {
            Font font = FontInstance.CreateFont(size);
            if(
                font.TryGetGlyphs(new CodePoint(codepoint1), out var glyphs1) &&
                font.TryGetGlyphs(new CodePoint(codepoint2), out var glyphs2)
                )
            {
                if (glyphs1.Count > 0 && glyphs2.Count > 0)
                {
                    var glyph1 = glyphs1[0];
                    var glyph2 = glyphs2[0];

                    if(font.TryGetKerningOffset(glyph1, glyph2, Dpi, out var offset))
                    {
                        return offset / font.FontMetrics.UnitsPerEm;
                    } else
                    {
                        return new Vector2(
                            (float)glyph1.GlyphMetrics.AdvanceWidth / font.FontMetrics.UnitsPerEm * size * PointsPerInch / Dpi, 
                            (float)glyph1.GlyphMetrics.AdvanceHeight / font.FontMetrics.UnitsPerEm * size * PointsPerInch / Dpi
                            );
                    }
                }
            }

            throw new Exception("Invalid glyph list");
        }

        public void DrawGlyphToImage(GlyphPath glyphPath, System.Drawing.Point location, Image<Rgba32> image)
        {
            var paths = glyphPath.Paths;
            if (paths == null)
            {
                return;
            }

            paths = paths.Translate(location.X - glyphPath.Bounds.X, location.Y - glyphPath.Bounds.Y);
            DrawColoredPaths(image, paths);
        }

        /// <summary>
        /// Draws a collection of paths with the given colors onto the image.
        /// </summary>
        private void DrawColoredPaths(Image<Rgba32> image, IPathCollection paths)
        {
            IEnumerator<IPath> pathEnumerator = paths.GetEnumerator();

            int i = 0;
            while (pathEnumerator.MoveNext())
            {
                IPath path = pathEnumerator.Current;
                image.Mutate(x => x.Fill(DrawingOptions, Color.White, path));
                i++;
            }
        }
    }
}
