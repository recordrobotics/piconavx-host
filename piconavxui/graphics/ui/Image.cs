using Silk.NET.OpenGL;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;

namespace piconavx.ui.graphics.ui
{
    public class Image : UIController
    {
        public class ImageMaterial : UIMaterial
        {
            public Texture? Texture { get; set; }

            public ImageMaterial() : base(new Shader("assets/shaders/uivertex.glsl", "assets/shaders/uiimage.glsl"))
            {
            }

            public override void Use(RenderProperties properties)
            {
                base.Use(properties);
                (Texture ?? Texture.White).Bind(TextureUnit.Texture0);
                Shader.SetUniform("TextureSampler", 0);
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

        private static ImageMaterial? material;

        public Image(Canvas canvas) : base(canvas)
        {
            bounds = new RectangleF(0, 0, 0, 0);
            material ??= Scene.AddResource(new ImageMaterial());
            color = new Rgba32(200, 200, 200, 255);
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

            Tessellator.Quad.DrawQuad(bounds, color);
            Tessellator.Quad.Flush();
        }

        public override void Subscribe()
        {
        }

        public override void Unsubscribe()
        {
        }
    }
}
