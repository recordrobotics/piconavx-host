using System.Drawing;
using System.Numerics;

namespace piconavx.ui
{
    public static class UIExtensions
    {
        public static RectangleF Transform(this RectangleF rect, Matrix4x4 matrix)
        {
            Vector4 topLeft = new Vector4(rect.Left, rect.Top, 0, 1);
            Vector4 bottomRight = new Vector4(rect.Right, rect.Bottom, 0, 1);
            topLeft = Vector4.Transform(topLeft, matrix);
            bottomRight = Vector4.Transform(bottomRight, matrix);
            return new RectangleF(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }

        public static RectangleF AsFloat(this Rectangle rect)
        {
            return new(rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}
