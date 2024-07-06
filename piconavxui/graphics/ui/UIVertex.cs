using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public struct UIVertex
    {
        public Vector2 Position;
        public Rgba32 Color;
        public Vector2 TexCoords;
    }
}
