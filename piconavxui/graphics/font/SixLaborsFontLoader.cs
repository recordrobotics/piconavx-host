using FontStashSharp.Interfaces;

namespace piconavx.ui.graphics.font
{
    public class SixLaborsFontLoader : IFontLoader
    {
        public IFontSource Load(byte[] data)
        {
            return new SixLaborsFontSource(data);
        }
    }
}
