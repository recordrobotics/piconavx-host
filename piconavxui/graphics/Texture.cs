using piconavx.ui.controllers;
using piconavx.ui.graphics.ui;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

namespace piconavx.ui.graphics
{
    public class Texture : IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static Texture White;
        public static Texture Black;
        public static Texture UVTest;
        public static Texture RoundedRect;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


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
        public readonly int Width;
        public readonly int Height;

        private TextureWrapMode wrapModeU = TextureWrapMode.Repeat;
        private TextureWrapMode wrapModeV = TextureWrapMode.Repeat;
        private TextureWrapMode wrapModeW = TextureWrapMode.Repeat;

        public TextureWrapMode WrapModeU { get=>wrapModeU; set=>wrapModeU = value; }
        public TextureWrapMode WrapModeV { get=>wrapModeV; set=>wrapModeV = value; }
        public TextureWrapMode WrapModeW { get=>wrapModeW; set=>wrapModeW = value; }
        public TextureWrapMode WrapMode { get => wrapModeU; set => wrapModeU = wrapModeV = wrapModeW = value; }

        private FilterMode filterMode = FilterMode.Trilinear;
        public FilterMode FilterMode { get => filterMode; set => filterMode = value; }

        private Insets border = new Insets(0);
        public Insets Border { get => border; set => border = value; }

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

                Width = img.Width;
                Height = img.Height;
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

            Width = (int)width;
            Height = (int)height;
        }

        public unsafe Texture(uint width, uint height)
        {
            _handle = Window.GL.GenTexture();
            Bind();

            Window.GL.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
            SetParameters();

            Width = (int)width;
            Height = (int)height;
        }

        public unsafe void SetData(Rectangle bounds, byte[] data)
        {
            Bind();
            fixed (byte* d = &data[0])
            {
                Window.GL.TexSubImage2D(TextureTarget.Texture2D, 0, bounds.Left, bounds.Top, (uint)bounds.Width, (uint)bounds.Height, PixelFormat.Rgba, PixelType.UnsignedByte, d);
            }
        }

        public unsafe void SetData(System.Drawing.Rectangle bounds, byte[] data)
        {
            SetData(new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height), data);
        }

        private GLEnum getWrapMode(TextureWrapMode mode)
        {
            switch (mode)
            {
                case TextureWrapMode.Repeat:
                    return GLEnum.Repeat;
                case TextureWrapMode.Clamp:
                    return GLEnum.ClampToEdge;
                case TextureWrapMode.Mirror:
                    return GLEnum.MirroredRepeat;
                case TextureWrapMode.MirrorOnce:
                    return GLEnum.MirrorClampToEdge;
            }
            return GLEnum.Repeat;
        }

        private GLEnum getFilterMode(FilterMode mode)
        {
            switch (mode)
            {
                case FilterMode.Point:
                    return GLEnum.Nearest;
                case FilterMode.Bilinear:
                    return GLEnum.Linear;
                case FilterMode.Trilinear:
                    return GLEnum.Linear;
            }
            return GLEnum.Linear;
        }

        private GLEnum getFilterModeMipmap(FilterMode mode)
        {
            switch (mode)
            {
                case FilterMode.Point:
                    return GLEnum.NearestMipmapLinear;
                case FilterMode.Bilinear:
                    return GLEnum.LinearMipmapNearest;
                case FilterMode.Trilinear:
                    return GLEnum.LinearMipmapLinear;
            }
            return GLEnum.Linear;
        }

        private void SetParameters()
        {
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)getWrapMode(wrapModeU));
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)getWrapMode(wrapModeV));
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)getWrapMode(wrapModeW));
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)getFilterModeMipmap(filterMode));
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)getFilterMode(filterMode));
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
            Window.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
            Window.GL.GenerateMipmap(TextureTarget.Texture2D);
        }

        public void UpdateSettings()
        {
            Bind();
            SetParameters();
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

    public enum TextureWrapMode
    {
        Repeat,
        Clamp,
        Mirror,
        MirrorOnce
    }

    public enum FilterMode
    {
        Point,
        Bilinear,
        Trilinear
    }
}
