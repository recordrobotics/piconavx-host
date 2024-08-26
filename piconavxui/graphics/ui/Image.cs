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
            public float HitTestAlphaClip { get; set; } = 0.5f;

            public ImageMaterial() : base(new Shader("assets/shaders/uivertex.glsl", "assets/shaders/uiimage.glsl"), new Shader("assets/shaders/uivertex.glsl", "assets/shaders/uiimage.hittest.glsl"))
            {
            }

            public override void Use(RenderProperties properties)
            {
                Use(properties, null);
            }

            public override void Use(RenderProperties properties, Transform? transform)
            {
                base.Use(properties, transform);
                (Texture ?? Texture.White).Bind(TextureUnit.Texture0);
                Shader.SetUniform("TextureSampler", 0);
            }

            public override void UseHitTest(Canvas canvas, byte id, Transform? transform)
            {
                base.UseHitTest(canvas, id, transform);
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
                Use(properties, null);
            }

            public override void Use(RenderProperties properties, Transform? transform)
            {
                base.Use(properties, transform);
                (Texture ?? Texture.White).Bind(TextureUnit.Texture0);
                (Mask ?? Texture.White).Bind(TextureUnit.Texture1);
                Shader.SetUniform("TextureSampler", 0);
                Shader.SetUniform("MaskTextureSampler", 1);
            }

            public override void UseHitTest(Canvas canvas, byte id, Transform? transform)
            {
                base.UseHitTest(canvas, id, transform);
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

        private UIColor color;
        public UIColor Color { get => color; set => color = value; }

        private float hitTestAlphaClip = 0.5f;
        public float HitTestAlphaClip { get => hitTestAlphaClip; set => hitTestAlphaClip = value; }

        private ImageType imageType = ImageType.Simple;
        public ImageType ImageType { get => imageType; set => imageType = value; }

        private ImageSegment visibleSegments = ImageSegment.All;
        public ImageSegment VisibleSegments { get => visibleSegments; set => visibleSegments = value; }

        private Size size = new Size(100, 100);
        public Size Size { get => size; set => size = value; }

        private Size sizeAlt = new Size(-1, -1);
        public Size SizeAlt { get => sizeAlt; set => sizeAlt = value; }

        public bool HasSizeAlt => sizeAlt.Width != -1 && sizeAlt.Height != -1;

        private bool preserveAspect = false;
        public bool PreserveAspect { get => preserveAspect; set => preserveAspect = value; }

        private bool flipHorizontal = false;
        public bool FlipHorizontal { get => flipHorizontal; set => flipHorizontal = value; }

        private bool flipVertical = false;
        public bool FlipVertical { get => flipVertical; set => flipVertical = value; }

        private static ImageMaterial? material;
        private static MaskedImageMaterial? maskedMaterial;

        public Image(Canvas canvas) : base(canvas)
        {
            bounds = new RectangleF(0, 0, 0, 0);
            material ??= Scene.AddResource(new ImageMaterial());
            maskedMaterial ??= Scene.AddResource(new MaskedImageMaterial());
            color = new SolidUIColor(new Rgba32(200, 200, 200, 255));
        }

        private void UVFlip<T>(ref T v0, ref T v1, bool hor, bool ver) where T : unmanaged, ITessellatorVertex<T>
        {
            if (!hor && !ver)
                return; 

            float left = v0.TexCoords.X;
            left = MathF.Min(left, v1.TexCoords.X);

            float right = v0.TexCoords.X;
            right = MathF.Max(right, v1.TexCoords.X);

            float top = v0.TexCoords.Y;
            top = MathF.Min(top, v1.TexCoords.Y);

            float bottom = v0.TexCoords.Y;
            bottom = MathF.Max(bottom, v1.TexCoords.Y);

            v0.TexCoords = new Vector2(hor ? (right + left - v0.TexCoords.X) : v0.TexCoords.X, ver ? (bottom + top - v0.TexCoords.Y) : v0.TexCoords.Y);
            v1.TexCoords = new Vector2(hor ? (right + left - v1.TexCoords.X) : v1.TexCoords.X, ver ? (bottom + top - v1.TexCoords.Y) : v1.TexCoords.Y);

            left = v0.TexCoordsAlt.X;
            left = MathF.Min(left, v1.TexCoordsAlt.X);

            right = v0.TexCoordsAlt.X;
            right = MathF.Max(right, v1.TexCoordsAlt.X);

            top = v0.TexCoordsAlt.Y;
            top = MathF.Min(top, v1.TexCoordsAlt.Y);

            bottom = v0.TexCoordsAlt.Y;
            bottom = MathF.Max(bottom, v1.TexCoordsAlt.Y);

            v0.TexCoords = new Vector2(hor ? (right + left - v0.TexCoordsAlt.X) : v0.TexCoordsAlt.X, ver ? (bottom + top - v0.TexCoordsAlt.Y) : v0.TexCoordsAlt.Y);
            v1.TexCoords = new Vector2(hor ? (right + left - v1.TexCoordsAlt.X) : v1.TexCoordsAlt.X, ver ? (bottom + top - v1.TexCoordsAlt.Y) : v1.TexCoordsAlt.Y);
        }

        private void UVFlip<T>(ref T v0, ref T v1, ref T v2, bool hor, bool ver) where T : unmanaged, ITessellatorVertex<T>
        {
            if (!hor && !ver)
                return;

            float left = v0.TexCoords.X;
            left = MathF.Min(left, v1.TexCoords.X);
            left = MathF.Min(left, v2.TexCoords.X);

            float right = v0.TexCoords.X;
            right = MathF.Max(right, v1.TexCoords.X);
            right = MathF.Max(right, v2.TexCoords.X);

            float top = v0.TexCoords.Y;
            top = MathF.Min(top, v1.TexCoords.Y);
            top = MathF.Min(top, v2.TexCoords.Y);

            float bottom = v0.TexCoords.Y;
            bottom = MathF.Max(bottom, v1.TexCoords.Y);
            bottom = MathF.Max(bottom, v2.TexCoords.Y);

            v0.TexCoords = new Vector2(hor ? (right + left - v0.TexCoords.X) : v0.TexCoords.X, ver ? (bottom + top - v0.TexCoords.Y) : v0.TexCoords.Y);
            v1.TexCoords = new Vector2(hor ? (right + left - v1.TexCoords.X) : v1.TexCoords.X, ver ? (bottom + top - v1.TexCoords.Y) : v1.TexCoords.Y);
            v2.TexCoords = new Vector2(hor ? (right + left - v2.TexCoords.X) : v2.TexCoords.X, ver ? (bottom + top - v2.TexCoords.Y) : v2.TexCoords.Y);

            left = v0.TexCoordsAlt.X;
            left = MathF.Min(left, v1.TexCoordsAlt.X);
            left = MathF.Min(left, v2.TexCoordsAlt.X);

            right = v0.TexCoordsAlt.X;
            right = MathF.Max(right, v1.TexCoordsAlt.X);
            right = MathF.Max(right, v2.TexCoordsAlt.X);

            top = v0.TexCoordsAlt.Y;
            top = MathF.Min(top, v1.TexCoordsAlt.Y);
            top = MathF.Min(top, v2.TexCoordsAlt.Y);

            bottom = v0.TexCoordsAlt.Y;
            bottom = MathF.Max(bottom, v1.TexCoordsAlt.Y);
            bottom = MathF.Max(bottom, v2.TexCoordsAlt.Y);

            v0.TexCoordsAlt = new Vector2(hor ? (right + left - v0.TexCoordsAlt.X) : v0.TexCoordsAlt.X, ver ? (bottom + top - v0.TexCoordsAlt.Y) : v0.TexCoords.Y);
            v1.TexCoordsAlt = new Vector2(hor ? (right + left - v1.TexCoordsAlt.X) : v1.TexCoordsAlt.X, ver ? (bottom + top - v1.TexCoordsAlt.Y) : v1.TexCoords.Y);
            v2.TexCoordsAlt = new Vector2(hor ? (right + left - v2.TexCoordsAlt.X) : v2.TexCoordsAlt.X, ver ? (bottom + top - v2.TexCoordsAlt.Y) : v2.TexCoords.Y);
        }

        private void UVFlip<T>(ref T v0, ref T v1, ref T v2, ref T v3, bool hor, bool ver) where T : unmanaged, ITessellatorVertex<T>
        {
            if (!hor && !ver)
                return;

            float left = v0.TexCoords.X;
            left = MathF.Min(left, v1.TexCoords.X);
            left = MathF.Min(left, v2.TexCoords.X);
            left = MathF.Min(left, v3.TexCoords.X);

            float right = v0.TexCoords.X;
            right = MathF.Max(right, v1.TexCoords.X);
            right = MathF.Max(right, v2.TexCoords.X);
            right = MathF.Max(right, v3.TexCoords.X);

            float top = v0.TexCoords.Y;
            top = MathF.Min(top, v1.TexCoords.Y);
            top = MathF.Min(top, v2.TexCoords.Y);
            top = MathF.Min(top, v3.TexCoords.Y);

            float bottom = v0.TexCoords.Y;
            bottom = MathF.Max(bottom, v1.TexCoords.Y);
            bottom = MathF.Max(bottom, v2.TexCoords.Y);
            bottom = MathF.Max(bottom, v3.TexCoords.Y);

            v0.TexCoords = new Vector2(hor ? (right + left - v0.TexCoords.X) : v0.TexCoords.X, ver ? (bottom + top - v0.TexCoords.Y) : v0.TexCoords.Y);
            v1.TexCoords = new Vector2(hor ? (right + left - v1.TexCoords.X) : v1.TexCoords.X, ver ? (bottom + top - v1.TexCoords.Y) : v1.TexCoords.Y);
            v2.TexCoords = new Vector2(hor ? (right + left - v2.TexCoords.X) : v2.TexCoords.X, ver ? (bottom + top - v2.TexCoords.Y) : v2.TexCoords.Y);
            v3.TexCoords = new Vector2(hor ? (right + left - v3.TexCoords.X) : v3.TexCoords.X, ver ? (bottom + top - v3.TexCoords.Y) : v3.TexCoords.Y);

            left = v0.TexCoordsAlt.X;
            left = MathF.Min(left, v1.TexCoordsAlt.X);
            left = MathF.Min(left, v2.TexCoordsAlt.X);
            left = MathF.Min(left, v3.TexCoordsAlt.X);

            right = v0.TexCoordsAlt.X;
            right = MathF.Max(right, v1.TexCoordsAlt.X);
            right = MathF.Max(right, v2.TexCoordsAlt.X);
            right = MathF.Max(right, v3.TexCoordsAlt.X);

            top = v0.TexCoordsAlt.Y;
            top = MathF.Min(top, v1.TexCoordsAlt.Y);
            top = MathF.Min(top, v2.TexCoordsAlt.Y);
            top = MathF.Min(top, v3.TexCoordsAlt.Y);

            bottom = v0.TexCoordsAlt.Y;
            bottom = MathF.Max(bottom, v1.TexCoordsAlt.Y);
            bottom = MathF.Max(bottom, v2.TexCoordsAlt.Y);
            bottom = MathF.Max(bottom, v3.TexCoordsAlt.Y);

            v0.TexCoordsAlt = new Vector2(hor ? (right + left - v0.TexCoordsAlt.X) : v0.TexCoordsAlt.X, ver ? (bottom + top - v0.TexCoordsAlt.Y) : v0.TexCoordsAlt.Y);
            v1.TexCoordsAlt = new Vector2(hor ? (right + left - v1.TexCoordsAlt.X) : v1.TexCoordsAlt.X, ver ? (bottom + top - v1.TexCoordsAlt.Y) : v1.TexCoordsAlt.Y);
            v2.TexCoordsAlt = new Vector2(hor ? (right + left - v2.TexCoordsAlt.X) : v2.TexCoordsAlt.X, ver ? (bottom + top - v2.TexCoordsAlt.Y) : v2.TexCoordsAlt.Y);
            v3.TexCoordsAlt = new Vector2(hor ? (right + left - v3.TexCoordsAlt.X) : v3.TexCoordsAlt.X, ver ? (bottom + top - v3.TexCoordsAlt.Y) : v3.TexCoordsAlt.Y);
        }

        private void UVFlip<T>(ref T v0, ref T v1, ref T v2, ref T v3, RectangleF rc, bool hor, bool ver) where T : unmanaged, ITessellatorVertex<T>
        {
            if (!hor && !ver)
                return;

            v0.TexCoords = new Vector2(hor ? (rc.Right + rc.Left - v0.TexCoords.X) : v0.TexCoords.X, ver ? (rc.Bottom + rc.Top - v0.TexCoords.Y) : v0.TexCoords.Y);
            v1.TexCoords = new Vector2(hor ? (rc.Right + rc.Left - v1.TexCoords.X) : v1.TexCoords.X, ver ? (rc.Bottom + rc.Top - v1.TexCoords.Y) : v1.TexCoords.Y);
            v2.TexCoords = new Vector2(hor ? (rc.Right + rc.Left - v2.TexCoords.X) : v2.TexCoords.X, ver ? (rc.Bottom + rc.Top - v2.TexCoords.Y) : v2.TexCoords.Y);
            v3.TexCoords = new Vector2(hor ? (rc.Right + rc.Left - v3.TexCoords.X) : v3.TexCoords.X, ver ? (rc.Bottom + rc.Top - v3.TexCoords.Y) : v3.TexCoords.Y);

            v0.TexCoordsAlt = new Vector2(hor ? (rc.Right + rc.Left - v0.TexCoordsAlt.X) : v0.TexCoordsAlt.X, ver ? (rc.Bottom + rc.Top - v0.TexCoordsAlt.Y) : v0.TexCoordsAlt.Y);
            v1.TexCoordsAlt = new Vector2(hor ? (rc.Right + rc.Left - v1.TexCoordsAlt.X) : v1.TexCoordsAlt.X, ver ? (rc.Bottom + rc.Top - v1.TexCoordsAlt.Y) : v1.TexCoordsAlt.Y);
            v2.TexCoordsAlt = new Vector2(hor ? (rc.Right + rc.Left - v2.TexCoordsAlt.X) : v2.TexCoordsAlt.X, ver ? (rc.Bottom + rc.Top - v2.TexCoordsAlt.Y) : v2.TexCoordsAlt.Y);
            v3.TexCoordsAlt = new Vector2(hor ? (rc.Right + rc.Left - v3.TexCoordsAlt.X) : v3.TexCoordsAlt.X, ver ? (rc.Bottom + rc.Top - v3.TexCoordsAlt.Y) : v3.TexCoordsAlt.Y);
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
                    if (!visibleSegments.HasFlag(ImageSegment.Middle))
                        break;

                    if (!preserveAspect && !flipHorizontal && !flipVertical)
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

                        if (flipHorizontal || flipVertical)
                        {
                            UVFlip(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, flipHorizontal, flipVertical);
                        }

                        if (preserveAspect)
                        {
                            UVAspect(ref topLeft, true, true);
                            UVAspect(ref topRight, true, true);
                            UVAspect(ref bottomLeft, true, true);
                            UVAspect(ref bottomRight, true, true);
                        }

                        tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                    }
                    break;
                case ImageType.Tiled:
                    {
                        if (!visibleSegments.HasFlag(ImageSegment.Middle))
                            break;

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

                        if (flipHorizontal || flipVertical)
                        {
                            UVFlip(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, flipHorizontal, flipVertical);
                        }

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
                        if (visibleSegments.HasFlag(ImageSegment.TopLeft))
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

                            if (flipHorizontal || flipVertical)
                            {
                                UVFlip(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, new RectangleF(0, 0, 1, 1), flipHorizontal, flipVertical);
                            }

                            tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }

                        // T
                        if (visibleSegments.HasFlag(ImageSegment.Top))
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
                                  new Vector2(bounds.Right - (HasSizeAlt ? sizeAlt.Width : size.Width), bounds.Top),
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
                                  new Vector2(bounds.Right - (HasSizeAlt ? sizeAlt.Width : size.Width), bounds.Top + size.Height),
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

                                if (flipHorizontal || flipVertical)
                                {
                                    UVFlip(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, new RectangleF(0, 0, 1, 1), flipHorizontal, flipVertical);
                                }

                                tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                            }
                        }

                        // TR
                        if (visibleSegments.HasFlag(ImageSegment.TopRight))
                        {
                            T topLeft = T.Create(
                              new Vector2(bounds.Right - (HasSizeAlt ? sizeAlt.Width : size.Width), bounds.Top),
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
                              new Vector2(bounds.Right - (HasSizeAlt ? sizeAlt.Width : size.Width), bounds.Top + (HasSizeAlt ? sizeAlt.Height : size.Height)),
                              color,
                              new Vector2(borderUV.Z, borderUV.Y),
                              new Vector2(altUV.Z, altUV.Y)
                            );

                            T bottomRight = T.Create(
                              new Vector2(bounds.Right, bounds.Top + (HasSizeAlt ? sizeAlt.Height : size.Height)),
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

                            if (flipHorizontal || flipVertical)
                            {
                                UVFlip(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, new RectangleF(0, 0, 1, 1), flipHorizontal, flipVertical);
                            }

                            tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }

                        // BL
                        if (visibleSegments.HasFlag(ImageSegment.BottomLeft))
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

                            if (flipHorizontal || flipVertical)
                            {
                                UVFlip(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, new RectangleF(0, 0, 1, 1), flipHorizontal, flipVertical);
                            }

                            tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }

                        // B
                        if (visibleSegments.HasFlag(ImageSegment.Bottom))
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
                                  new Vector2(bounds.Right - (HasSizeAlt ? sizeAlt.Width : size.Width), bounds.Bottom - size.Height),
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
                                  new Vector2(bounds.Right - (HasSizeAlt ? sizeAlt.Width : size.Width), bounds.Bottom),
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

                                if (flipHorizontal || flipVertical)
                                {
                                    UVFlip(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, new RectangleF(0, 0, 1, 1), flipHorizontal, flipVertical);
                                }

                                tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                            }
                        }

                        // BR
                        if (visibleSegments.HasFlag(ImageSegment.BottomRight))
                        {
                            T topLeft = T.Create(
                              new Vector2(bounds.Right - (HasSizeAlt ? sizeAlt.Width : size.Width), bounds.Bottom - (HasSizeAlt ? sizeAlt.Height : size.Height)),
                              color,
                              new Vector2(borderUV.Z, borderUV.W),
                              new Vector2(altUV.Z, altUV.W)
                            );

                            T topRight = T.Create(
                              new Vector2(bounds.Right, bounds.Bottom - (HasSizeAlt ? sizeAlt.Height : size.Height)),
                              color,
                              new Vector2(1, borderUV.W),
                              new Vector2(1, altUV.W)
                            );

                            T bottomLeft = T.Create(
                              new Vector2(bounds.Right - (HasSizeAlt ? sizeAlt.Width : size.Width), bounds.Bottom),
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

                            if (flipHorizontal || flipVertical)
                            {
                                UVFlip(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, new RectangleF(0, 0, 1, 1), flipHorizontal, flipVertical);
                            }

                            tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                        }

                        // L
                        if (visibleSegments.HasFlag(ImageSegment.Left))
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

                                if (flipHorizontal || flipVertical)
                                {
                                    UVFlip(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, new RectangleF(0, 0, 1, 1), flipHorizontal, flipVertical);
                                }

                                tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                            }
                        }

                        // R
                        if (visibleSegments.HasFlag(ImageSegment.Right))
                        {
                            if (bounds.Bottom - size.Height > bounds.Top + size.Height)
                            {
                                T topLeft = T.Create(
                                  new Vector2(bounds.Right - (HasSizeAlt ? sizeAlt.Width : size.Width), bounds.Top + (HasSizeAlt ? sizeAlt.Height : size.Height)),
                                  color,
                                  new Vector2(borderUV.Z, borderUV.Y),
                                  new Vector2(altUV.Z, altUV.Y)
                                );

                                T topRight = T.Create(
                                  new Vector2(bounds.Right, bounds.Top + (HasSizeAlt ? sizeAlt.Height : size.Height)),
                                  color,
                                  new Vector2(1, borderUV.Y),
                                  new Vector2(1, altUV.Y)
                                );

                                T bottomLeft = T.Create(
                                  new Vector2(bounds.Right - (HasSizeAlt ? sizeAlt.Width : size.Width), bounds.Bottom - (HasSizeAlt ? sizeAlt.Height : size.Height)),
                                  color,
                                  new Vector2(borderUV.Z, borderUV.W),
                                  new Vector2(altUV.Z, altUV.W)
                                );

                                T bottomRight = T.Create(
                                  new Vector2(bounds.Right, bounds.Bottom - (HasSizeAlt ? sizeAlt.Height : size.Height)),
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

                                if (flipHorizontal || flipVertical)
                                {
                                    UVFlip(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, new RectangleF(0, 0, 1, 1), flipHorizontal, flipVertical);
                                }

                                tessellator.DrawQuad(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight);
                            }
                        }

                        // M
                        if (visibleSegments.HasFlag(ImageSegment.Middle))
                        {
                            T topLeft = T.Create(
                              new Vector2(bounds.Left + size.Width, bounds.Top + size.Height),
                              color,
                              new Vector2(borderUV.X, borderUV.Y),
                              new Vector2(altUV.X, altUV.Y)
                            );

                            T topRight = T.Create(
                              new Vector2(bounds.Right - (HasSizeAlt ? sizeAlt.Width : size.Width), bounds.Top + size.Height),
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
                                new Vector2(bounds.Right - (HasSizeAlt ? sizeAlt.Width : size.Width), bounds.Bottom - size.Height),
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

                            if (flipHorizontal || flipVertical)
                            {
                                UVFlip(ref topLeft, ref topRight, ref bottomLeft, ref bottomRight, new RectangleF(0, 0, 1, 1), flipHorizontal, flipVertical);
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
                material.Use(properties, Transform);
            }
            else if (maskedMaterial != null && mask != null)
            {
                maskedMaterial.Texture = texture;
                maskedMaterial.Mask = mask;
                maskedMaterial.Use(properties, Transform);
            }
            else
            {
                UIMaterial.DefaultMaterial.Use(properties, Transform);
            }

            if (mask != null)
            {
                Tessellate(Tessellator.QuadExt);
                Tessellator.QuadExt.Flush();
            }
            else
            {
                Tessellate(Tessellator.Quad);
                Tessellator.Quad.Flush();
            }
        }

        public override void HitTest(byte id)
        {
            if (material != null && mask == null)
            {
                material.Texture = texture;
                material.HitTestAlphaClip = hitTestAlphaClip;
                material.UseHitTest(Canvas, id, Transform);
            }
            else if (maskedMaterial != null && mask != null)
            {
                maskedMaterial.Texture = texture;
                maskedMaterial.Mask = mask;
                maskedMaterial.HitTestAlphaClip = hitTestAlphaClip;
                maskedMaterial.UseHitTest(Canvas, id, Transform);
            }
            else
            {
                return;
            }

            if (mask != null)
            {
                Tessellate(Tessellator.QuadExt);
                Tessellator.QuadExt.Flush();
            }
            else
            {
                Tessellate(Tessellator.Quad);
                Tessellator.Quad.Flush();
            }
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

    [Flags]
    public enum ImageSegment
    {
        TopLeft = 1 << 0,
        Top = 1 << 1,
        TopRight = 1 << 2,
        Left = 1 << 3,
        Middle = 1 << 4,
        Right = 1 << 5,
        BottomLeft = 1 << 6,
        Bottom = 1 << 7,
        BottomRight = 1 << 8,

        None = 0,
        All = TopLeft | Top | TopRight | Left | Middle | Right | BottomLeft | Bottom | BottomRight
    }
}
