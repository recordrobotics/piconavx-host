using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;

namespace piconavx.ui.graphics.font
{
    internal class GlyphPath
    {
        public float Size;
        public int Codepoint;
        public Rectangle Bounds;
        public IPathCollection? Paths;
    }
}
