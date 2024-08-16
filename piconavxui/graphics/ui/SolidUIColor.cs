using FontStashSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace piconavx.ui.graphics.ui
{
    public class SolidUIColor : UIColor
    {
        public override Rgba32 Value => value;

        private Rgba32 value;

        public SolidUIColor(Rgba32 value)
        {
            this.value = value;
        }

        public SolidUIColor(FSColor value)
        {
            this.value = new(value.ToVector4());
        }

        public void SetColor(Rgba32 value)
        {
            this.value = value;
        }

        public void SetColor(FSColor value)
        {
            this.value = new(value.ToVector4());
        }

        public static readonly SolidUIColor White = new SolidUIColor(FSColor.White);
    }
}
