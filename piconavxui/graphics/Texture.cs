using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using Image = SixLabors.ImageSharp.Image;

namespace piconavx.ui.graphics
{
    public class Texture : IDisposable
    {
        public static Texture White;
        public static Texture Black;

        public static Texture CreateSolidColor(Rgba32 color, uint width, uint height)
        {
            byte[] data = new byte[width * height * 4];
            for (uint i = 0; i < width * height; i++)
            {
                data[i * 4] = color.R;
                data[i * 4 + 1] = color.G;
                data[i * 4 + 2] = color.B;
                data[i * 4 + 3] = color.A;
            }

            return new Texture(data, width, height);
        }

        public static Texture CreateWhite(uint width, uint height)
        {
            return CreateSolidColor(new Rgba32(255,255,255,255), width, height);
        }

        public static Texture CreateBlack(uint width, uint height)
        {
            return CreateSolidColor(new Rgba32(0, 0, 0, 255), width, height);
        }

        private uint _handle;

        public string? Path { get; set; }
        public TextureType Type { get; }

        public unsafe Texture(string path, TextureType type = TextureType.None)
        {
            Path = path;
            Type = type;
            _handle = Window.GL.GenTexture();
            Bind();

            using (var img = Image.Load<Rgba32>(EmbeddedResource.GetResource(path)!))
            {
                Window.GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)img.Width, (uint)img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);

                img.ProcessPixelRows(accessor =>
                {
                    for (int y = 0; y < accessor.Height; y++)
                    {
                        fixed (void* data = accessor.GetRowSpan(y))
                        {
                            Window.GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint)accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                        }
                    }
                });
            }

            SetParameters();
        }

        public unsafe Texture(Span<byte> data, uint width, uint height)
        {
            _handle = Window.GL.GenTexture();
            Bind();

            fixed (void* d = &data[0])
            {
                Window.GL.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
                SetParameters();
            }
        }

        private void SetParameters()
        {
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
            Window.GL.GenerateMipmap(TextureTarget.Texture2D);
        }

        public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
        {
            Window.GL.ActiveTexture(textureSlot);
            Window.GL.BindTexture(TextureTarget.Texture2D, _handle);
        }

        public void Dispose()
        {
            Window.GL.DeleteTexture(_handle);
        }
    }
}
