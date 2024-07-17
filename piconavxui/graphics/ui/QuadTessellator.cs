using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public class QuadTessellator<T> : Tessellator where T : unmanaged, ITessellatorVertex<T>
    {
        private const int MAX_ELEMENTS = 512;
        private const int MAX_VERTICES = MAX_ELEMENTS * 4;
        private const int MAX_INDICES = MAX_ELEMENTS * 6;

        private BufferObject<T>? _vertexBuffer;
        private BufferObject<short>? _indexBuffer;
        private VertexArrayObject<T, short>? _vao;
        private T[]? _vertexData;
        private static short[]? indexData;
        private int _vertexIndex = 0;

        private static short[] GenerateIndexArray()
        {
            short[] result = new short[MAX_INDICES];
            for (int i = 0, j = 0; i < MAX_INDICES; i += 6, j += 4)
            {
                result[i] = (short)(j);
                result[i + 1] = (short)(j + 1);
                result[i + 2] = (short)(j + 2);
                result[i + 3] = (short)(j + 3);
                result[i + 4] = (short)(j + 2);
                result[i + 5] = (short)(j + 1);
            }
            return result;
        }

        public override void CreateResources()
        {
            indexData ??= GenerateIndexArray();
            _vertexData = new T[MAX_VERTICES];

            _vertexBuffer = new BufferObject<T>(MAX_VERTICES, BufferTargetARB.ArrayBuffer, true);
            _indexBuffer = new BufferObject<short>(indexData, BufferTargetARB.ElementArrayBuffer, false);

            _vao = new VertexArrayObject<T, short>(_vertexBuffer, _indexBuffer);
            _vao.Bind();

            T.SetUpVertexAttributePointers(_vao);
        }

        public override void Dispose()
        {
            _vao?.Dispose();
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _vertexData = null;
        }

        public void DrawRectangleOutline(RectangleF bounds, Rgba32 color, float thickness)
        {
            DrawQuad(new RectangleF(bounds.X, bounds.Y, thickness, bounds.Height), color);
            DrawQuad(new RectangleF(bounds.X + thickness, bounds.Y, bounds.Width - thickness - thickness, thickness), color);
            DrawQuad(new RectangleF(bounds.Right - thickness, bounds.Y, thickness, bounds.Height), color);
            DrawQuad(new RectangleF(bounds.X + thickness, bounds.Bottom - thickness, bounds.Width - thickness - thickness, thickness), color);
        }

        public void DrawQuad(ref T topLeft, ref T topRight, ref T bottomLeft, ref T bottomRight)
        {
            if (_vertexData == null) // lazy resource creation only when this tessellator is used
                CreateResources();

            if (_vertexIndex + 4 >= MAX_VERTICES)
                Flush(); // force flush after reaching limit

            _vertexData![_vertexIndex++] = topLeft;
            _vertexData[_vertexIndex++] = topRight;
            _vertexData[_vertexIndex++] = bottomLeft;
            _vertexData[_vertexIndex++] = bottomRight;
        }

        public void DrawQuad(RectangleF bounds, Rgba32 colorTopLeft, Rgba32 colorTopRight, Rgba32 colorBottomLeft, Rgba32 colorBottomRight)
        {
            T topLeft = T.Create(
                new Vector2(bounds.Left, bounds.Top),
                colorTopLeft,
                new Vector2(0, 0),
                new Vector2(0, 0)
                );

            T topRight = T.Create(
                new Vector2(bounds.Right, bounds.Top),
                colorTopRight,
                new Vector2(1, 0),
                new Vector2(1, 0)
                );

            T bottomLeft = T.Create(
                new Vector2(bounds.Left, bounds.Bottom),
                colorBottomLeft,
                new Vector2(0, 1),
                new Vector2(0, 1)
                );

            T bottomRight = T.Create(
                new Vector2(bounds.Right, bounds.Bottom),
                colorBottomRight,
                new Vector2(1, 1),
                new Vector2(1, 1)
                );

            DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
        }

        public void DrawQuad(RectangleF bounds, Rgba32 color)
        {
            DrawQuad(bounds, color, color, color, color);
        }

        public override void Flush()
        {
            if (_vertexData == null) // lazy resource creation only when this tessellator is used
                CreateResources();

            if (_vertexIndex == 0)
            {
                return;
            }

            _vao?.Bind();
            _indexBuffer?.Bind();

            _vertexBuffer?.SetData(_vertexData!, 0, _vertexIndex);

            unsafe
            {
                Window.GL.DrawElements(PrimitiveType.Triangles, (uint)(_vertexIndex * 6 / 4), DrawElementsType.UnsignedShort, null);
            }

            Reset();
        }

        public override void Reset()
        {
            _vertexIndex = 0;
        }
    }
}
