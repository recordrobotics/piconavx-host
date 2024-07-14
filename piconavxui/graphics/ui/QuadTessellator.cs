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
    public class QuadTessellator : Tessellator
    {
        private const int MAX_ELEMENTS = 128;
        private const int MAX_VERTICES = MAX_ELEMENTS * 4;
        private const int MAX_INDICES = MAX_ELEMENTS * 6;

        private BufferObject<UIVertex>? _vertexBuffer;
        private BufferObject<short>? _indexBuffer;
        private VertexArrayObject<UIVertex, short>? _vao;
        private UIVertex[]? _vertexData;
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
            _vertexData = new UIVertex[MAX_VERTICES];

            _vertexBuffer = new BufferObject<UIVertex>(MAX_VERTICES, BufferTargetARB.ArrayBuffer, true);
            _indexBuffer = new BufferObject<short>(indexData, BufferTargetARB.ElementArrayBuffer, false);

            _vao = new VertexArrayObject<UIVertex, short>(_vertexBuffer, _indexBuffer);
            _vao.Bind();

            _vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 1, false, 0);
            _vao.VertexAttributePointer(1, 4, VertexAttribPointerType.UnsignedByte, 1, true, 2 * sizeof(float));
            _vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 1, false, 2 * sizeof(float) + 4 * sizeof(byte));
        }

        public override void Dispose()
        {
            _vao?.Dispose();
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _vertexData = null;
        }

        public void DrawQuad(ref UIVertex topLeft, ref UIVertex topRight, ref UIVertex bottomLeft, ref UIVertex bottomRight)
        {
            if (_vertexData == null) // lazy resource creation only when this tessellator is used
                CreateResources();

            _vertexData![_vertexIndex++] = topLeft;
            _vertexData[_vertexIndex++] = topRight;
            _vertexData[_vertexIndex++] = bottomLeft;
            _vertexData[_vertexIndex++] = bottomRight;
        }

        public void DrawQuad(RectangleF bounds, Rgba32 colorTopLeft, Rgba32 colorTopRight, Rgba32 colorBottomLeft, Rgba32 colorBottomRight)
        {
            UIVertex topLeft = new UIVertex()
            {
                Position = new Vector2(bounds.Left, bounds.Top),
                Color = colorTopLeft,
                TexCoords = new Vector2(0, 0)
            };

            UIVertex topRight = new UIVertex()
            {
                Position = new Vector2(bounds.Right, bounds.Top),
                Color = colorTopRight,
                TexCoords = new Vector2(1, 0)
            };

            UIVertex bottomLeft = new UIVertex()
            {
                Position = new Vector2(bounds.Left, bounds.Bottom),
                Color = colorBottomLeft,
                TexCoords = new Vector2(0, 1)
            };

            UIVertex bottomRight = new UIVertex()
            {
                Position = new Vector2(bounds.Right, bounds.Bottom),
                Color = colorBottomRight,
                TexCoords = new Vector2(1, 1)
            };

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
