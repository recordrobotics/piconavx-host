using FontStashSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Numerics;

namespace piconavx.ui.graphics.ui
{
    public class Label : UIController
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

        private string text;
        public string Text
        {
            get => text; set => text = value;
        }

        private Func<string>? textFunc;
        public Func<string>? TextDelegate { get => textFunc; set => textFunc = value; }

        private float fontSize = 12.0f;
        public float FontSize
        {
            get => fontSize; set => fontSize = value;
        }

        private FontFace font = FontFace.InterRegular;
        public FontFace Font { get => font; set => font = value; }

        private bool autoSize = true;
        public bool AutoSize { get => autoSize; set => autoSize = value; }

        private RectangleF bounds;
        public override RectangleF Bounds { get => bounds; set => bounds = value; }

        private Vector2 renderOffset;
        public Vector2 RenderOffset { get => renderOffset; set => renderOffset = value; }

        private FSColor color;
        public FSColor Color
        {
            get => color; set => color = value;
        }

        public Label(string text, Canvas canvas) : base(canvas)
        {
            RaycastTransparency = RaycastTransparency.Hidden; // don't perform input events on text
            this.text = text;
            color = FSColor.White;
            bounds = GetAutoSizeBounds();
        }

        public Label(Func<string> textDelegate, Canvas canvas) : this(textDelegate.Invoke(), canvas)
        {
            this.textFunc = textDelegate;
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
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
        }

        public override void Unsubscribe()
        {
            Scene.Update -= Scene_Update;
        }

        private void Scene_Update(double deltaTime)
        {
            if (autoSize)
            {
                bounds = GetAutoSizeBounds();
            }

            if (textFunc != null)
                text = textFunc.Invoke();
        }
    }
}
