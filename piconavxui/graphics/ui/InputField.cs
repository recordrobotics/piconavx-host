using FontStashSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace piconavx.ui.graphics.ui
{
    public class InputField : TextInputBase
    {
        private float fontSize = 12.0f;
        public float FontSize
        {
            get => fontSize; set => fontSize = value;
        }

        private FontFace font = FontFace.InterRegular;
        public FontFace Font { get => font; set => font = value; }

        private Vector2 renderOffset;
        public Vector2 RenderOffset { get => renderOffset; set => renderOffset = value; }

        private FSColor color;
        public FSColor Color
        {
            get => color; set => color = value;
        }

        StringBuilder text = new("Hello");

        private int zIndex = 0;
        public override int ZIndex
        {
            get => zIndex; set
            {
                zIndex = value;
                UpdateZIndex();
                Canvas.InvalidateHierarchy();
            }
        }

        private RectangleF bounds;
        public override RectangleF Bounds { get => bounds; set => bounds = value; }
        public override bool IsRenderable => supportsInputEvents;

        public bool SupportsInputEvents { get => supportsInputEvents; set => supportsInputEvents = value; }
        private bool supportsInputEvents = false;

        public InputField(Canvas canvas) : base(canvas)
        {
            bounds = new RectangleF(0, 0, 0, 0);
        }

        public override void Render(double deltaTime, RenderProperties properties)
        {
            var fontSystem = Window.FontSystems[this.font];
            var font = fontSystem.GetFont(fontSize);

            var glyphs = font.GetGlyphs(text, new Vector2(bounds.X, bounds.Y), renderOffset, new Vector2(fontSystem.FontResolutionFactor, fontSystem.FontResolutionFactor));
            UIMaterial.ColorMaterial.Use(properties);
            foreach (var glyph in glyphs)
            {
                Tessellator.Quad.DrawQuad(glyph.Bounds.AsFloat().Transform(Transform.Matrix), new Rgba32(255, 0, 255, 255));
            }
            Tessellator.Quad.Flush();

            Window.FontRenderer.Begin(Transform.Matrix);
            font.DrawText(Window.FontRenderer, text, new Vector2(bounds.X, bounds.Y), color, 0, renderOffset, new Vector2(fontSystem.FontResolutionFactor, fontSystem.FontResolutionFactor));
            Window.FontRenderer.End();
        }

        public override void Subscribe()
        {
            base.Subscribe();
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
        }

        protected virtual void UpdateZIndex() { }

        protected override void AddChar(char chr, int index)
        {
            if (index < text.Length)
                text.Replace(text[index], chr, index, 1);
            else
                text.Append(chr);
        }

        protected override void RemoveChars(int index, int length)
        {
            text.Remove(index, length);
        }
    }
}
