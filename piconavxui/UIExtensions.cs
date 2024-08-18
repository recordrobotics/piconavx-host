using piconavx.ui.graphics.ui;
using System.Drawing;
using System.Numerics;

namespace piconavx.ui
{
    public static class UIExtensions
    {
        public static T Max<T>(T a, T b, T c, T d) where T : INumber<T>
        {
            return T.Max(a, T.Max(b, T.Max(c, d)));
        }

        public static T Min<T>(T a, T b, T c, T d) where T : INumber<T>
        {
            return T.Min(a, T.Min(b, T.Min(c, d)));
        }

        public static RectangleF Transform(this RectangleF rect, Matrix4x4 matrix)
        {
            Vector4 topLeft = new Vector4(rect.Left, rect.Top, 0, 1);
            Vector4 topRight = new Vector4(rect.Right, rect.Top, 0, 1);
            Vector4 bottomLeft = new Vector4(rect.Left, rect.Bottom, 0, 1);
            Vector4 bottomRight = new Vector4(rect.Right, rect.Bottom, 0, 1);
            topLeft = Vector4.Transform(topLeft, matrix);
            topRight = Vector4.Transform(topRight, matrix);
            bottomLeft = Vector4.Transform(bottomLeft, matrix);
            bottomRight = Vector4.Transform(bottomRight, matrix);

            float left = Min(topLeft.X, topRight.X, bottomLeft.X, bottomRight.X);
            float top = Min(topLeft.Y, topRight.Y, bottomLeft.Y, bottomRight.Y);
            float right = Max(topLeft.X, topRight.X, bottomLeft.X, bottomRight.X);
            float bottom = Max(topLeft.Y, topRight.Y, bottomLeft.Y, bottomRight.Y);

            return new RectangleF(left, top, right - left, bottom - top);
        }

        public static RectangleF AsFloat(this Rectangle rect)
        {
            return new(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static void SetOrigin(this UIController controller, Vector2 origin)
        {
            controller.Transform.SetOrigin(controller, origin);
        }

        public static string InsertFromEnd(this string str, int index, string value)
        {
            return str.Insert(str.Length - index, value);
        }
    }
}
