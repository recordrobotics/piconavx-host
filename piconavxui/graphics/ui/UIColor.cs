using FontStashSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace piconavx.ui.graphics.ui
{
    public abstract class UIColor
    {
        public abstract Rgba32 Value { get; }

        public static implicit operator Rgba32(UIColor color)
        {
            return color.Value;
        }

        public static implicit operator FSColor(UIColor color)
        {
            return new(color.Value.ToVector4());
        }
    }
}
