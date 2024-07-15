using Silk.NET.OpenGL;

namespace piconavx.ui.graphics
{
    public class Framebuffer : IDisposable
    {
        private uint _handle;
        private uint _texHandle;

        public uint Width { get; private set; }
        public uint Height { get; private set; }

        public InternalFormat InternalFormat { get; }
        public PixelFormat PixelFormat { get; }
        public PixelType PixelType { get; }

        public unsafe Framebuffer(uint width, uint height, InternalFormat internalFormat, PixelFormat pixelFormat, PixelType pixelType)
        {
            InternalFormat = internalFormat;
            PixelFormat = pixelFormat;
            PixelType = pixelType;

            Width = width; 
            Height = height;

            _handle = Window.GL.GenFramebuffer();
            Bind();
            CreateTexture();
        }

        private unsafe void CreateTexture()
        {
            _texHandle = Window.GL.GenTexture();
            Window.GL.BindTexture(TextureTarget.Texture2D, _texHandle);
            Window.GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat, Math.Max(1, Width), Math.Max(1, Height), 0, PixelFormat, PixelType, null);
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToBorder);
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToBorder);
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);

            Window.GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _texHandle, 0);
            Window.GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            Window.GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

            if (Window.GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
                throw new Exception("Error creating framebuffer");
        }

        public void Bind()
        {
            Window.GL.BindFramebuffer(FramebufferTarget.Framebuffer, _handle);
        }

        public void SetSize(uint width, uint height)
        {
            Width = width;
            Height = height;
            Bind();
            Window.GL.DeleteTexture(_texHandle);
            CreateTexture();
        }

        public void Dispose()
        {
            Window.GL.DeleteTexture(_texHandle);
            Window.GL.DeleteFramebuffer(_handle);
        }
    }
}
