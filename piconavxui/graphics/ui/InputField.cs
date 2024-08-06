using FontStashSharp;
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
    public class InputField : TextInputBase
    {
        private int zIndex = 0;
        public override int ZIndex
        {
            get => zIndex; set
            {
                zIndex = value;
                Canvas.InvalidateHierarchy();
            }
        }

        private StringBuilder text;
        public StringBuilder Text
        {
            get => text; set { text = value; InvalidateGlyphs(); }
        }

        private float fontSize = 12.0f;
        public float FontSize
        {
            get => fontSize; set { fontSize = value; InvalidateGlyphs(); }
        }

        private FontFace font = FontFace.InterRegular;
        public FontFace Font { get => font; set { font = value; InvalidateGlyphs(); } }

        private bool autoSize = true;
        public bool AutoSize { get => autoSize; set => autoSize = value; }

        private RectangleF bounds;
        public override RectangleF Bounds { get => bounds; set { bounds = value; InvalidateGlyphs(); } }

        private Vector2 renderOffset;
        public Vector2 RenderOffset { get => renderOffset; set { renderOffset = value; InvalidateGlyphs(); } }

        private FSColor color;
        public FSColor Color
        {
            get => color; set => color = value;
        }

        public InputField(string text, Canvas canvas) : base(canvas)
        {
            RaycastTransparency = RaycastTransparency.Hidden; // don't perform input events on text
            this.text = new(text);
            color = FSColor.White;
            bounds = GetAutoSizeBounds();
            InvalidateGlyphs();
        }

        public RectangleF GetAutoSizeBounds()
        {
            var fontSystem = Window.FontSystems[this.font];
            var font = fontSystem.GetFont(fontSize);
            Vector2 size = font.MeasureString(text, new Vector2(fontSystem.FontResolutionFactor, fontSystem.FontResolutionFactor));
            return new RectangleF(bounds.X, bounds.Y, size.X, size.Y);
        }

        public override void Render(double deltaTime, RenderProperties properties)
        {
            UIMaterial.ColorMaterial.Use(properties);
            foreach (var glyph in Glyphs)
            {
                Tessellator.Quad.DrawQuad(glyph.Bounds.AsFloat().Transform(Transform.Matrix), new Rgba32(255, 0, 255, 255));
            }
            Tessellator.Quad.Flush();

            var fontSystem = Window.FontSystems[this.font];
            var font = fontSystem.GetFont(fontSize);

            Window.FontRenderer.Begin(Transform.Matrix);
            font.DrawText(Window.FontRenderer, text, new Vector2(bounds.X, bounds.Y), color, 0, renderOffset, new Vector2(fontSystem.FontResolutionFactor, fontSystem.FontResolutionFactor));
            Window.FontRenderer.End();
        }

        public override void Subscribe()
        {
            base.Subscribe();
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            Scene.Update -= Scene_Update;
        }

        private void Scene_Update(double deltaTime)
        {
            if (autoSize)
            {
                bounds = GetAutoSizeBounds();
            }
        }

        protected override void AddChar(char chr, int index)
        {
            if (index < text.Length)
                text.Replace(text[index], chr, index, 1);
            else
                text.Append(chr);
            InvalidateGlyphs();
        }

        protected override void RemoveChars(int index, int length)
        {
            text.Remove(index, length);
            InvalidateGlyphs();
        }

        protected override void InvalidateGlyphs()
        {
            var fontSystem = Window.FontSystems[this.font];
            var font = fontSystem.GetFont(fontSize);

            Glyphs = font.GetGlyphs(text, new Vector2(bounds.X, bounds.Y), renderOffset, new Vector2(fontSystem.FontResolutionFactor, fontSystem.FontResolutionFactor));
        }
    }
}
