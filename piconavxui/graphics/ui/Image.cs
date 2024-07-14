using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Numerics;

namespace piconavx.ui.graphics.ui
{
    public class Image : UIController
    {
        public class ImageMaterial : UIMaterial
        {
            public Texture? Texture { get; set; }

            public ImageMaterial() : base(new Shader("assets/shaders/uivertex.glsl", "assets/shaders/uiimage.glsl"), new Shader("assets/shaders/uivertex.glsl", "assets/shaders/uiimage.hittest.glsl"))
            {
            }

            public override void Use(RenderProperties properties)
            {
                base.Use(properties);
                (Texture ?? Texture.White).Bind(TextureUnit.Texture0);
                Shader.SetUniform("TextureSampler", 0);
            }

            public override void UseHitTest(Canvas canvas, byte id)
            {
                base.UseHitTest(canvas, id);
                (Texture ?? Texture.White).Bind(TextureUnit.Texture0);
                if (HitTestShader != null)
                    HitTestShader.SetUniform("TextureSampler", 0);
            }
        }

        private int zIndex = 0;
        public override int ZIndex
        {
            get => zIndex; set
            {
                zIndex = value;
                Canvas.InvalidateHierarchy();
            }
        }

        private RectangleF bounds;
        public override RectangleF Bounds { get => bounds; set => bounds = value; }

        private Texture? texture;
        public Texture? Texture { get => texture; set => texture = value; }

        private Rgba32 color;
        public Rgba32 Color { get => color; set => color = value; }

        private ImageType imageType;
        public ImageType ImageType { get => imageType; set => imageType = value; }

        private Size size = new Size(100, 100);
        public Size Size { get => size; set => size = value; }

        private static ImageMaterial? material;

        public Image(Canvas canvas) : base(canvas)
        {
            bounds = new RectangleF(0, 0, 0, 0);
            material ??= Scene.AddResource(new ImageMaterial());
            color = new Rgba32(200, 200, 200, 255);
        }

        private void Tessellate()
        {
            switch (imageType)
            {
                case ImageType.Simple:
                    Tessellator.Quad.DrawQuad(bounds, color);
                    break;
                case ImageType.Tiled:
                    {
                        Vector2 uvScale = new Vector2(bounds.Width / Size.Width, bounds.Height / Size.Height);

                        UIVertex topLeft = new UIVertex()
                        {
                            Position = new Vector2(bounds.Left, bounds.Top),
                            Color = color,
                            TexCoords = new Vector2(0, 0)
                        };

                        UIVertex topRight = new UIVertex()
                        {
                            Position = new Vector2(bounds.Right, bounds.Top),
                            Color = color,
                            TexCoords = new Vector2(uvScale.X, 0)
                        };

                        UIVertex bottomLeft = new UIVertex()
                        {
                            Position = new Vector2(bounds.Left, bounds.Bottom),
                            Color = color,
                            TexCoords = new Vector2(0, uvScale.Y)
                        };

                        UIVertex bottomRight = new UIVertex()
                        {
                            Position = new Vector2(bounds.Right, bounds.Bottom),
                            Color = color,
                            TexCoords = uvScale
                        };

                        Tessellator.Quad.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                    }
                    break;
                case ImageType.Sliced:
                    {
                        Vector4 borderUV = Texture != null ?
                            new Vector4
                            (Texture.Border.Left / Texture.Width,
                            Texture.Border.Top / Texture.Height,
                            (1.0f - Texture.Border.Right / Texture.Width),
                            (1.0f - Texture.Border.Bottom / Texture.Height)
                            ) :
                            new Vector4(1);

                        // TL
                        {
                            UIVertex topLeft = new UIVertex()
                            {
                                Position = new Vector2(bounds.Left, bounds.Top),
                                Color = color,
                                TexCoords = new Vector2(0, 0)
                            };

                            UIVertex topRight = new UIVertex()
                            {
                                Position = new Vector2(bounds.Left + size.Width, bounds.Top),
                                Color = color,
                                TexCoords = new Vector2(borderUV.X, 0)
                            };

                            UIVertex bottomLeft = new UIVertex()
                            {
                                Position = new Vector2(bounds.Left, bounds.Top + size.Height),
                                Color = color,
                                TexCoords = new Vector2(0, borderUV.Y)
                            };

                            UIVertex bottomRight = new UIVertex()
                            {
                                Position = new Vector2(bounds.Left + size.Width, bounds.Top + size.Height),
                                Color = color,
                                TexCoords = new Vector2(borderUV.X, borderUV.Y)
                            };

                            Tessellator.Quad.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }

                        // T
                        {
                            if (bounds.Right - size.Width > bounds.Left + size.Width)
                            {
                                UIVertex topLeft = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Left + size.Width, bounds.Top),
                                    Color = color,
                                    TexCoords = new Vector2(borderUV.X, 0)
                                };

                                UIVertex topRight = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Right - size.Width, bounds.Top),
                                    Color = color,
                                    TexCoords = new Vector2(borderUV.Z, 0)
                                };

                                UIVertex bottomLeft = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Left + size.Width, bounds.Top + size.Height),
                                    Color = color,
                                    TexCoords = new Vector2(borderUV.X, borderUV.Y)
                                };

                                UIVertex bottomRight = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Right - size.Width, bounds.Top + size.Height),
                                    Color = color,
                                    TexCoords = new Vector2(borderUV.Z, borderUV.Y)
                                };

                                Tessellator.Quad.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                            }
                        }

                        // TR
                        {
                            UIVertex topLeft = new UIVertex()
                            {
                                Position = new Vector2(bounds.Right - size.Width, bounds.Top),
                                Color = color,
                                TexCoords = new Vector2(borderUV.Z, 0)
                            };

                            UIVertex topRight = new UIVertex()
                            {
                                Position = new Vector2(bounds.Right, bounds.Top),
                                Color = color,
                                TexCoords = new Vector2(1, 0)
                            };

                            UIVertex bottomLeft = new UIVertex()
                            {
                                Position = new Vector2(bounds.Right - size.Width, bounds.Top + size.Height),
                                Color = color,
                                TexCoords = new Vector2(borderUV.Z, borderUV.Y)
                            };

                            UIVertex bottomRight = new UIVertex()
                            {
                                Position = new Vector2(bounds.Right, bounds.Top + size.Height),
                                Color = color,
                                TexCoords = new Vector2(1, borderUV.Y)
                            };

                            Tessellator.Quad.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }

                        // BL
                        {
                            UIVertex topLeft = new UIVertex()
                            {
                                Position = new Vector2(bounds.Left, bounds.Bottom - size.Height),
                                Color = color,
                                TexCoords = new Vector2(0, borderUV.W)
                            };

                            UIVertex topRight = new UIVertex()
                            {
                                Position = new Vector2(bounds.Left + size.Width, bounds.Bottom - size.Height),
                                Color = color,
                                TexCoords = new Vector2(borderUV.X, borderUV.W)
                            };

                            UIVertex bottomLeft = new UIVertex()
                            {
                                Position = new Vector2(bounds.Left, bounds.Bottom),
                                Color = color,
                                TexCoords = new Vector2(0, 1)
                            };

                            UIVertex bottomRight = new UIVertex()
                            {
                                Position = new Vector2(bounds.Left + size.Width, bounds.Bottom),
                                Color = color,
                                TexCoords = new Vector2(borderUV.X, 1)
                            };

                            Tessellator.Quad.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }

                        // B
                        {
                            if (bounds.Right - size.Width > bounds.Left + size.Width)
                            {
                                UIVertex topLeft = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Left + size.Width, bounds.Bottom - size.Height),
                                    Color = color,
                                    TexCoords = new Vector2(borderUV.X, borderUV.W)
                                };

                                UIVertex topRight = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Right - size.Width, bounds.Bottom - size.Height),
                                    Color = color,
                                    TexCoords = new Vector2(borderUV.Z, borderUV.W)
                                };

                                UIVertex bottomLeft = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Left + size.Width, bounds.Bottom),
                                    Color = color,
                                    TexCoords = new Vector2(borderUV.X, 1)
                                };

                                UIVertex bottomRight = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Right - size.Width, bounds.Bottom),
                                    Color = color,
                                    TexCoords = new Vector2(borderUV.Z, 1)
                                };

                                Tessellator.Quad.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                            }
                        }

                        // BR
                        {
                            UIVertex topLeft = new UIVertex()
                            {
                                Position = new Vector2(bounds.Right - size.Width, bounds.Bottom - size.Height),
                                Color = color,
                                TexCoords = new Vector2(borderUV.Z, borderUV.W)
                            };

                            UIVertex topRight = new UIVertex()
                            {
                                Position = new Vector2(bounds.Right, bounds.Bottom - size.Height),
                                Color = color,
                                TexCoords = new Vector2(1, borderUV.W)
                            };

                            UIVertex bottomLeft = new UIVertex()
                            {
                                Position = new Vector2(bounds.Right - size.Width, bounds.Bottom),
                                Color = color,
                                TexCoords = new Vector2(borderUV.Z, 1)
                            };

                            UIVertex bottomRight = new UIVertex()
                            {
                                Position = new Vector2(bounds.Right, bounds.Bottom),
                                Color = color,
                                TexCoords = new Vector2(1, 1)
                            };

                            Tessellator.Quad.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }

                        // L
                        {
                            if (bounds.Bottom - size.Height > bounds.Top + size.Height)
                            {
                                UIVertex topLeft = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Left, bounds.Top + size.Height),
                                    Color = color,
                                    TexCoords = new Vector2(0, borderUV.Y)
                                };

                                UIVertex topRight = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Left + size.Width, bounds.Top + size.Height),
                                    Color = color,
                                    TexCoords = new Vector2(borderUV.X, borderUV.Y)
                                };

                                UIVertex bottomLeft = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Left, bounds.Bottom - size.Height),
                                    Color = color,
                                    TexCoords = new Vector2(0, borderUV.W)
                                };

                                UIVertex bottomRight = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Left + size.Width, bounds.Bottom - size.Height),
                                    Color = color,
                                    TexCoords = new Vector2(borderUV.X, borderUV.W)
                                };

                                Tessellator.Quad.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                            }
                        }

                        // R
                        {
                            if (bounds.Bottom - size.Height > bounds.Top + size.Height)
                            {
                                UIVertex topLeft = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Right - size.Width, bounds.Top + size.Height),
                                    Color = color,
                                    TexCoords = new Vector2(borderUV.Z, borderUV.Y)
                                };

                                UIVertex topRight = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Right, bounds.Top + size.Height),
                                    Color = color,
                                    TexCoords = new Vector2(1, borderUV.Y)
                                };

                                UIVertex bottomLeft = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Right - size.Width, bounds.Bottom - size.Height),
                                    Color = color,
                                    TexCoords = new Vector2(borderUV.Z, borderUV.W)
                                };

                                UIVertex bottomRight = new UIVertex()
                                {
                                    Position = new Vector2(bounds.Right, bounds.Bottom - size.Height),
                                    Color = color,
                                    TexCoords = new Vector2(1, borderUV.W)
                                };

                                Tessellator.Quad.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                            }
                        }

                        // M
                        {
                            UIVertex topLeft = new UIVertex()
                            {
                                Position = new Vector2(bounds.Left + size.Width, bounds.Top + size.Height),
                                Color = color,
                                TexCoords = new Vector2(borderUV.X, borderUV.Y)
                            };

                            UIVertex topRight = new UIVertex()
                            {
                                Position = new Vector2(bounds.Right - size.Width, bounds.Top + size.Height),
                                Color = color,
                                TexCoords = new Vector2(borderUV.Z, borderUV.Y)
                            };

                            UIVertex bottomLeft = new UIVertex()
                            {
                                Position = new Vector2(bounds.Left + size.Width, bounds.Bottom - size.Height),
                                Color = color,
                                TexCoords = new Vector2(borderUV.X, borderUV.W)
                            };

                            UIVertex bottomRight = new UIVertex()
                            {
                                Position = new Vector2(bounds.Right - size.Width, bounds.Bottom - size.Height),
                                Color = color,
                                TexCoords = new Vector2(borderUV.Z, borderUV.W)
                            };

                            Tessellator.Quad.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }
                    }
                    break;
            }
        }

        public override void Render(double deltaTime, RenderProperties properties)
        {
            if (material == null)
                UIMaterial.DefaultMaterial.Use(properties);
            else
            {
                material.Texture = texture;
                material.Use(properties);
            }

            Tessellate();
            Tessellator.Quad.Flush();
        }

        public override void HitTest(byte id)
        {
            if (material == null)
                return;

            material.Texture = texture;
            material.UseHitTest(Canvas, id);
            Tessellate();
            Tessellator.Quad.Flush();
        }

        public override void Subscribe()
        {
        }

        public override void Unsubscribe()
        {
        }
    }

    public enum ImageType
    {
        Simple,
        Sliced,
        Tiled
    }
}
