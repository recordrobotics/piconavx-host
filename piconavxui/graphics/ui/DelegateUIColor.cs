using SixLabors.ImageSharp.PixelFormats;

namespace piconavx.ui.graphics.ui
{
    internal class DelegateUIColor : UIColor
    {
        public override Rgba32 Value => valueFunc();

        private readonly Func<Rgba32> valueFunc;

        internal DelegateUIColor(Func<Rgba32> valueFunc)
        {
            this.valueFunc = valueFunc;
        }
    }
}
