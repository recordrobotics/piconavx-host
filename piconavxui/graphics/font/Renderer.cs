using FontStashSharp.Interfaces;
using Silk.NET.Core.Native;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.font
{
    public class Renderer : IFontStashRenderer2, IDisposable
    {
        private const int MAX_SPRITES = 2048;
        private const int MAX_VERTICES = MAX_SPRITES * 4;
        private const int MAX_INDICES = MAX_SPRITES * 6;

        private readonly Shader _shader;
        private readonly BufferObject<VertexPositionColorTexture> _vertexBuffer;
        private readonly BufferObject<short> _indexBuffer;
        private readonly VertexArrayObject<VertexPositionColorTexture, short> _vao;
        private readonly VertexPositionColorTexture[] _vertexData = new VertexPositionColorTexture[MAX_VERTICES];
        private object? _lastTexture;
        private int _vertexIndex = 0;

        private readonly Texture2DManager _textureManager;

        public ITexture2DManager TextureManager => _textureManager;

        private static readonly short[] indexData = GenerateIndexArray();

        public unsafe Renderer()
        {
            _textureManager = new Texture2DManager();

            _vertexBuffer = new BufferObject<VertexPositionColorTexture>(MAX_VERTICES, BufferTargetARB.ArrayBuffer, true);
            _indexBuffer = new BufferObject<short>(indexData, BufferTargetARB.ElementArrayBuffer, false);

            _shader = new Shader("assets/shaders/fontvert.glsl", "assets/shaders/font.glsl");
            _shader.Use();

            _vao = new VertexArrayObject<VertexPositionColorTexture, short>(_vertexBuffer, _indexBuffer);
            _vao.Bind();

            _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 1, false, 0);
            _vao.VertexAttributePointer(1, 4, VertexAttribPointerType.UnsignedByte, 1, true, 3 * sizeof(float));
            _vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, 1, false, 3 * sizeof(float) + 4 * sizeof(byte));
        }

        ~Renderer() => Dispose(false);
        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _vao.Dispose();
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _shader.Dispose();
        }

        public void Begin(Matrix4x4 transformMatrix)
        {
            Window.GL.Disable(EnableCap.DepthTest);
            Window.GL.Enable(EnableCap.Blend);
            Window.GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);

            _shader.Use();
            _shader.SetUniform("TextureSampler", 0);

            var transform = Matrix4x4.CreateOrthographicOffCenter(0, Window.Current.Internal.FramebufferSize.X, Window.Current.Internal.FramebufferSize.Y, 0, 0, -1);
            _shader.SetUniform("uMatrix", transform);
            _shader.SetUniform("tMatrix", transformMatrix);

            _vao.Bind();
            _indexBuffer.Bind();
            _vertexBuffer.Bind();
        }

        public void DrawQuad(object texture, ref VertexPositionColorTexture topLeft, ref VertexPositionColorTexture topRight, ref VertexPositionColorTexture bottomLeft, ref VertexPositionColorTexture bottomRight)
        {
            if (_lastTexture != texture)
            {
                FlushBuffer();
            }

            _vertexData[_vertexIndex++] = topLeft;
            _vertexData[_vertexIndex++] = topRight;
            _vertexData[_vertexIndex++] = bottomLeft;
            _vertexData[_vertexIndex++] = bottomRight;

            _lastTexture = texture;
        }

        public void End()
        {
            FlushBuffer();
        }

        private unsafe void FlushBuffer()
        {
            if (_vertexIndex == 0 || _lastTexture == null)
            {
                return;
            }

            _vertexBuffer.SetData(_vertexData, 0, _vertexIndex);

            var texture = (Texture)_lastTexture;
            texture.Bind();

            Window.GL.DrawElements(PrimitiveType.Triangles, (uint)(_vertexIndex * 6 / 4), DrawElementsType.UnsignedShort, null);
            _vertexIndex = 0;
        }

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
    }
}
