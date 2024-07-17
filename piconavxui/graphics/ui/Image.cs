using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace piconavx.ui.graphics.ui
{
    public class Image : UIController
    {
        public class ImageMaterial : UIMaterial
        {
            public Texture? Texture { get; set; }
            public float HitTestAlphaClip { get; set; } = 0.5f;

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
                {
                    HitTestShader.SetUniform("TextureSampler", 0);
                    HitTestShader.SetUniform("uAlphaClip", HitTestAlphaClip);
                }
            }
        }

        public class MaskedImageMaterial : UIMaterial
        {
            public Texture? Texture { get; set; }
            public Texture? Mask { get; set; }
            public float HitTestAlphaClip { get; set; } = 0.5f;

            public MaskedImageMaterial() : base(new Shader("assets/shaders/uivertex.glsl", "assets/shaders/uiimagemasked.glsl"), new Shader("assets/shaders/uivertex.glsl", "assets/shaders/uiimagemasked.hittest.glsl"))
            {
            }

            public override void Use(RenderProperties properties)
            {
                base.Use(properties);
                (Texture ?? Texture.White).Bind(TextureUnit.Texture0);
                (Mask ?? Texture.White).Bind(TextureUnit.Texture1);
                Shader.SetUniform("TextureSampler", 0);
                Shader.SetUniform("MaskTextureSampler", 1);
            }

            public override void UseHitTest(Canvas canvas, byte id)
            {
                base.UseHitTest(canvas, id);
                (Texture ?? Texture.White).Bind(TextureUnit.Texture0);
                (Mask ?? Texture.White).Bind(TextureUnit.Texture1);
                if (HitTestShader != null)
                {
                    HitTestShader.SetUniform("TextureSampler", 0);
                    HitTestShader.SetUniform("MaskTextureSampler", 1);
                    HitTestShader.SetUniform("uAlphaClip", HitTestAlphaClip);
                }
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

        private Texture? mask;
        public Texture? Mask { get => mask; set => mask = value; }

        private Rgba32 color;
        public Rgba32 Color { get => color; set => color = value; }

        private float hitTestAlphaClip = 0.5f;
        public float HitTestAlphaClip { get => hitTestAlphaClip; set => hitTestAlphaClip = value; }

        private ImageType imageType;
        public ImageType ImageType { get => imageType; set => imageType = value; }

        private Size size = new Size(100, 100);
        public Size Size { get => size; set => size = value; }

        private bool preserveAspect = false;
        public bool PreserveAspect { get => preserveAspect; set => preserveAspect = value; }

        private static ImageMaterial? material;
        private static MaskedImageMaterial? maskedMaterial;

        public Image(Canvas canvas) : base(canvas)
        {
            bounds = new RectangleF(0, 0, 0, 0);
            material ??= Scene.AddResource(new ImageMaterial());
            maskedMaterial ??= Scene.AddResource(new MaskedImageMaterial());
            color = new Rgba32(200, 200, 200, 255);
        }

        private void UVAspect<T>(ref T vertex, bool uv1, bool uv2) where T : unmanaged, ITessellatorVertex<T>
        {
            float boundsAspect = bounds.Width / bounds.Height;

            if (texture != null && uv1)
            {
                float aspect = (float)texture.Width / texture.Height;
                if (bounds.Width > bounds.Height)
                {
                    vertex.TexCoords -= new Vector2(0, 0.5f - boundsAspect / aspect / 2);
                    vertex.TexCoords *= new Vector2(1, aspect / boundsAspect);
                }
                else
                {
                    vertex.TexCoords -= new Vector2(0.5f - aspect / boundsAspect / 2, 0);
                    vertex.TexCoords *= new Vector2(aspect * boundsAspect, 1);
                }
            }

            if (mask != null && uv2)
            {
                float aspect = (float)mask.Width / mask.Height;
                if (bounds.Width > bounds.Height)
                {
                    vertex.TexCoordsAlt -= new Vector2(0, 0.5f - boundsAspect / aspect / 2);
                    vertex.TexCoordsAlt *= new Vector2(1, aspect / boundsAspect);
                }
                else
                {
                    vertex.TexCoordsAlt -= new Vector2(0.5f - aspect / boundsAspect / 2, 0);
                    vertex.TexCoordsAlt *= new Vector2(aspect * boundsAspect, 1);
                }
            }
        }

        private void Tessellate<T>(QuadTessellator<T> tessellator) where T : unmanaged, ITessellatorVertex<T>
        {
            switch (imageType)
            {
                case ImageType.Simple:
                    if (!preserveAspect)
                    {
                        tessellator.DrawQuad(bounds, color);
                    }
                    else
                    {
                        T topLeft = T.Create(
                            new Vector2(bounds.Left, bounds.Top),
                            color,
                            new Vector2(0, 0),
                            new Vector2(0, 0)
                            );

                        T topRight = T.Create(
                            new Vector2(bounds.Right, bounds.Top),
                            color,
                            new Vector2(1, 0),
                            new Vector2(1, 0)
                            );

                        T bottomLeft = T.Create(
                            new Vector2(bounds.Left, bounds.Bottom),
                            color,
                            new Vector2(0, 1),
                            new Vector2(0, 1)
                            );

                        T bottomRight = T.Create(
                            new Vector2(bounds.Right, bounds.Bottom),
                            color,
                            new Vector2(1, 1),
                            new Vector2(1, 1)
                            );

                        UVAspect(ref topLeft, true, true);
                        UVAspect(ref topRight, true, true);
                        UVAspect(ref bottomLeft, true, true);
                        UVAspect(ref bottomRight, true, true);

                        tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                    }
                    break;
                case ImageType.Tiled:
                    {
                        Vector2 uvScale = new Vector2(bounds.Width / Size.Width, bounds.Height / Size.Height);

                        T topLeft = T.Create(
                            new Vector2(bounds.Left, bounds.Top),
                            color,
                            new Vector2(0, 0),
                            new Vector2(0, 0)
                            );

                        T topRight = T.Create(
                            new Vector2(bounds.Right, bounds.Top),
                            color,
                            new Vector2(uvScale.X, 0),
                            new Vector2(1, 0)
                            );

                        T bottomLeft = T.Create(
                            new Vector2(bounds.Left, bounds.Bottom),
                            color,
                            new Vector2(0, uvScale.Y),
                            new Vector2(0, 1)
                            );

                        T bottomRight = T.Create(
                            new Vector2(bounds.Right, bounds.Bottom),
                            color,
                            uvScale,
                            new Vector2(1, 1)
                            );

                        if (preserveAspect)
                        {
                            UVAspect(ref topLeft, false, true);
                            UVAspect(ref topRight, false, true);
                            UVAspect(ref bottomLeft, false, true);
                            UVAspect(ref bottomRight, false, true);
                        }

                        tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
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

                        Vector4 altUV = Mask != null ?
                            new Vector4
                            (size.Width / bounds.Width,
                            size.Height / bounds.Height,
                            1.0f - size.Width / bounds.Width,
                            1.0f - size.Height / bounds.Height
                            ) :
                            new Vector4(1);

                        // TL
                        {
                            T topLeft = T.Create(
                              new Vector2(bounds.Left, bounds.Top),
                              color,
                              new Vector2(0, 0),
                              new Vector2(0, 0)
                            );

                            T topRight = T.Create(
                              new Vector2(bounds.Left + size.Width, bounds.Top),
                              color,
                              new Vector2(borderUV.X, 0),
                              new Vector2(altUV.X, 0)
                            );

                            T bottomLeft = T.Create(
                              new Vector2(bounds.Left, bounds.Top + size.Height),
                              color,
                              new Vector2(0, borderUV.Y),
                              new Vector2(0, altUV.Y)
                            );

                            T bottomRight = T.Create(
                              new Vector2(bounds.Left + size.Width, bounds.Top + size.Height),
                              color,
                              new Vector2(borderUV.X, borderUV.Y),
                              new Vector2(altUV.X, altUV.Y)
                            );

                            if (preserveAspect)
                            {
                                UVAspect(ref topLeft, false, true);
                                UVAspect(ref topRight, false, true);
                                UVAspect(ref bottomLeft, false, true);
                                UVAspect(ref bottomRight, false, true);
                            }

                            tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }

                        // T
                        {
                            if (bounds.Right - size.Width > bounds.Left + size.Width)
                            {
                                T topLeft = T.Create(
                                  new Vector2(bounds.Left + size.Width, bounds.Top),
                                  color,
                                  new Vector2(borderUV.X, 0),
                                  new Vector2(altUV.X, 0)
                                );

                                T topRight = T.Create(
                                  new Vector2(bounds.Right - size.Width, bounds.Top),
                                  color,
                                  new Vector2(borderUV.Z, 0),
                                  new Vector2(altUV.Z, 0)
                                );

                                T bottomLeft = T.Create(
                                  new Vector2(bounds.Left + size.Width, bounds.Top + size.Height),
                                  color,
                                  new Vector2(borderUV.X, borderUV.Y),
                                  new Vector2(altUV.X, altUV.Y)
                                );

                                T bottomRight = T.Create(
                                  new Vector2(bounds.Right - size.Width, bounds.Top + size.Height),
                                  color,
                                  new Vector2(borderUV.Z, borderUV.Y),
                                  new Vector2(altUV.Z, altUV.Y)
                                );

                                if (preserveAspect)
                                {
                                    UVAspect(ref topLeft, false, true);
                                    UVAspect(ref topRight, false, true);
                                    UVAspect(ref bottomLeft, false, true);
                                    UVAspect(ref bottomRight, false, true);
                                }

                                tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                            }
                        }

                        // TR
                        {
                            T topLeft = T.Create(
                              new Vector2(bounds.Right - size.Width, bounds.Top),
                              color,
                              new Vector2(borderUV.Z, 0),
                              new Vector2(altUV.Z, 0)
                            );

                            T topRight = T.Create(
                              new Vector2(bounds.Right, bounds.Top),
                              color,
                              new Vector2(1, 0),
                              new Vector2(1, 0)
                            );

                            T bottomLeft = T.Create(
                              new Vector2(bounds.Right - size.Width, bounds.Top + size.Height),
                              color,
                              new Vector2(borderUV.Z, borderUV.Y),
                              new Vector2(altUV.Z, altUV.Y)
                            );

                            T bottomRight = T.Create(
                              new Vector2(bounds.Right, bounds.Top + size.Height),
                              color,
                              new Vector2(1, borderUV.Y),
                              new Vector2(1, altUV.Y)
                            );

                            if (preserveAspect)
                            {
                                UVAspect(ref topLeft, false, true);
                                UVAspect(ref topRight, false, true);
                                UVAspect(ref bottomLeft, false, true);
                                UVAspect(ref bottomRight, false, true);
                            }

                            tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }

                        // BL
                        {
                            T topLeft = T.Create(
                              new Vector2(bounds.Left, bounds.Bottom - size.Height),
                              color,
                              new Vector2(0, borderUV.W),
                              new Vector2(0, altUV.W)
                            );

                            T topRight = T.Create(
                              new Vector2(bounds.Left + size.Width, bounds.Bottom - size.Height),
                              color,
                              new Vector2(borderUV.X, borderUV.W),
                              new Vector2(altUV.X, altUV.W)
                            );

                            T bottomLeft = T.Create(
                              new Vector2(bounds.Left, bounds.Bottom),
                              color,
                              new Vector2(0, 1),
                              new Vector2(0, 1)
                            );

                            T bottomRight = T.Create(
                              new Vector2(bounds.Left + size.Width, bounds.Bottom),
                              color,
                              new Vector2(borderUV.X, 1),
                              new Vector2(altUV.X, 1)
                            );

                            if (preserveAspect)
                            {
                                UVAspect(ref topLeft, false, true);
                                UVAspect(ref topRight, false, true);
                                UVAspect(ref bottomLeft, false, true);
                                UVAspect(ref bottomRight, false, true);
                            }

                            tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }

                        // B
                        {
                            if (bounds.Right - size.Width > bounds.Left + size.Width)
                            {
                                T topLeft = T.Create(
                                  new Vector2(bounds.Left + size.Width, bounds.Bottom - size.Height),
                                  color,
                                  new Vector2(borderUV.X, borderUV.W),
                                  new Vector2(altUV.X, altUV.W)
                                );

                                T topRight = T.Create(
                                  new Vector2(bounds.Right - size.Width, bounds.Bottom - size.Height),
                                  color,
                                  new Vector2(borderUV.Z, borderUV.W),
                                  new Vector2(altUV.Z, altUV.W)
                                );

                                T bottomLeft = T.Create(
                                  new Vector2(bounds.Left + size.Width, bounds.Bottom),
                                  color,
                                  new Vector2(borderUV.X, 1),
                                  new Vector2(altUV.X, 1)
                                );

                                T bottomRight = T.Create(
                                  new Vector2(bounds.Right - size.Width, bounds.Bottom),
                                  color,
                                  new Vector2(borderUV.Z, 1),
                                  new Vector2(altUV.Z, 1)
                                );

                                if (preserveAspect)
                                {
                                    UVAspect(ref topLeft, false, true);
                                    UVAspect(ref topRight, false, true);
                                    UVAspect(ref bottomLeft, false, true);
                                    UVAspect(ref bottomRight, false, true);
                                }

                                tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                            }
                        }

                        // BR
                        {
                            T topLeft = T.Create(
                              new Vector2(bounds.Right - size.Width, bounds.Bottom - size.Height),
                              color,
                              new Vector2(borderUV.Z, borderUV.W),
                              new Vector2(altUV.Z, altUV.W)
                            );

                            T topRight = T.Create(
                              new Vector2(bounds.Right, bounds.Bottom - size.Height),
                              color,
                              new Vector2(1, borderUV.W),
                              new Vector2(1, altUV.W)
                            );

                            T bottomLeft = T.Create(
                              new Vector2(bounds.Right - size.Width, bounds.Bottom),
                              color,
                              new Vector2(borderUV.Z, 1),
                              new Vector2(altUV.Z, 1)
                            );

                            T bottomRight = T.Create(
                              new Vector2(bounds.Right, bounds.Bottom),
                              color,
                              new Vector2(1, 1),
                              new Vector2(1, 1)
                            );

                            if (preserveAspect)
                            {
                                UVAspect(ref topLeft, false, true);
                                UVAspect(ref topRight, false, true);
                                UVAspect(ref bottomLeft, false, true);
                                UVAspect(ref bottomRight, false, true);
                            }

                            tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }

                        // L
                        {
                            if (bounds.Bottom - size.Height > bounds.Top + size.Height)
                            {
                                T topLeft = T.Create(
                                  new Vector2(bounds.Left, bounds.Top + size.Height),
                                  color,
                                  new Vector2(0, borderUV.Y),
                                  new Vector2(0, altUV.Y)
                                );

                                T topRight = T.Create(
                                  new Vector2(bounds.Left + size.Width, bounds.Top + size.Height),
                                  color,
                                  new Vector2(borderUV.X, borderUV.Y),
                                  new Vector2(altUV.X, altUV.Y)
                                );

                                T bottomLeft = T.Create(
                                  new Vector2(bounds.Left, bounds.Bottom - size.Height),
                                  color,
                                  new Vector2(0, borderUV.W),
                                  new Vector2(0, altUV.W)
                                );

                                T bottomRight = T.Create(
                                  new Vector2(bounds.Left + size.Width, bounds.Bottom - size.Height),
                                  color,
                                  new Vector2(borderUV.X, borderUV.W),
                                  new Vector2(altUV.X, altUV.W)
                                );

                                if (preserveAspect)
                                {
                                    UVAspect(ref topLeft, false, true);
                                    UVAspect(ref topRight, false, true);
                                    UVAspect(ref bottomLeft, false, true);
                                    UVAspect(ref bottomRight, false, true);
                                }

                                tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                            }
                        }

                        // R
                        {
                            if (bounds.Bottom - size.Height > bounds.Top + size.Height)
                            {
                                T topLeft = T.Create(
                                  new Vector2(bounds.Right - size.Width, bounds.Top + size.Height),
                                  color,
                                  new Vector2(borderUV.Z, borderUV.Y),
                                  new Vector2(altUV.Z, altUV.Y)
                                );

                                T topRight = T.Create(
                                  new Vector2(bounds.Right, bounds.Top + size.Height),
                                  color,
                                  new Vector2(1, borderUV.Y),
                                  new Vector2(1, altUV.Y)
                                );

                                T bottomLeft = T.Create(
                                  new Vector2(bounds.Right - size.Width, bounds.Bottom - size.Height),
                                  color,
                                  new Vector2(borderUV.Z, borderUV.W),
                                  new Vector2(altUV.Z, altUV.W)
                                );

                                T bottomRight = T.Create(
                                  new Vector2(bounds.Right, bounds.Bottom - size.Height),
                                  color,
                                  new Vector2(1, borderUV.W),
                                  new Vector2(1, altUV.W)
                                );

                                if (preserveAspect)
                                {
                                    UVAspect(ref topLeft, false, true);
                                    UVAspect(ref topRight, false, true);
                                    UVAspect(ref bottomLeft, false, true);
                                    UVAspect(ref bottomRight, false, true);
                                }

                                tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                            }
                        }

                        // M
                        {
                            T topLeft = T.Create(
                              new Vector2(bounds.Left + size.Width, bounds.Top + size.Height),
                              color,
                              new Vector2(borderUV.X, borderUV.Y),
                              new Vector2(altUV.X, altUV.Y)
                            );

                            T topRight = T.Create(
                              new Vector2(bounds.Right - size.Width, bounds.Top + size.Height),
                              color,
                              new Vector2(borderUV.Z, borderUV.Y),
                              new Vector2(altUV.Z, altUV.Y)
                            );

                            T bottomLeft = T.Create(
                              new Vector2(bounds.Left + size.Width, bounds.Bottom - size.Height),
                              color,
                              new Vector2(borderUV.X, borderUV.W),
                              new Vector2(altUV.X, altUV.W)
                            );

                            T bottomRight = T.Create(
                                new Vector2(bounds.Right - size.Width, bounds.Bottom - size.Height),
                                color,
                                new Vector2(borderUV.Z, borderUV.W),
                                new Vector2(altUV.Z, altUV.W)
                            );

                            if (preserveAspect)
                            {
                                UVAspect(ref topLeft, false, true);
                                UVAspect(ref topRight, false, true);
                                UVAspect(ref bottomLeft, false, true);
                                UVAspect(ref bottomRight, false, true);
                            }

                            tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }
                    }
                    break;
            }
        }

        public override void Render(double deltaTime, RenderProperties properties)
        {
            if (material != null && mask == null)
            {
                material.Texture = texture;
                material.Use(properties);
            }
            else if (maskedMaterial != null && mask != null)
            {
                maskedMaterial.Texture = texture;
                maskedMaterial.Mask = mask;
                maskedMaterial.Use(properties);
            }
            else
            {
                UIMaterial.DefaultMaterial.Use(properties);
            }

            Tessellate(Tessellator.QuadExt);
            Tessellator.QuadExt.Flush();
        }

        public override void HitTest(byte id)
        {
            if (material != null && mask == null)
            {
                material.Texture = texture;
                material.HitTestAlphaClip = hitTestAlphaClip;
                material.UseHitTest(Canvas, id);
            }
            else if (maskedMaterial != null && mask != null)
            {
                maskedMaterial.Texture = texture;
                maskedMaterial.Mask = mask;
                maskedMaterial.HitTestAlphaClip = hitTestAlphaClip;
                maskedMaterial.UseHitTest(Canvas, id);
            }
            else
            {
                return;
            }

            Tessellate(Tessellator.Quad);
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
