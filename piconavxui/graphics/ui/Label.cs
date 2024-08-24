using FontStashSharp;
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

        private TextSegment[]? segments = null;
        public TextSegment[]? Segments
        {
            get => segments; set => segments = value;
        }

        private Func<(string, TextSegment[]?)>? textFunc;
        public Func<(string, TextSegment[]?)>? TextDelegate { get => textFunc; set => textFunc = value; }

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

        private UIColor color;
        public UIColor Color
        {
            get => color; set => color = value;
        }

        public Label(string text, Canvas canvas) : this(text, null, canvas)
        {
        }

        public Label(string text, TextSegment[]? segments, Canvas canvas) : base(canvas)
        {
            RaycastTransparency = RaycastTransparency.Hidden; // don't perform input events on text
            this.text = text;
            this.segments = segments;
            color = SolidUIColor.White;
            bounds = GetAutoSizeBounds();
        }

        private Label((string, TextSegment[]?) tuple, Canvas canvas) : this(tuple.Item1, tuple.Item2, canvas)
        {
        }

        public Label(Func<(string, TextSegment[]?)> textDelegate, Canvas canvas) : this(textDelegate.Invoke(), canvas)
        {
            this.textFunc = textDelegate;
        }

        public RectangleF GetAutoSizeBounds()
        {
            var fontSystem = Window.FontSystems[this.font];
            var font = fontSystem.GetFont(fontSize);
            Vector2 size = font.MeasureString(text, new Vector2(fontSystem.FontResolutionFactor, fontSystem.FontResolutionFactor));
            return new RectangleF(bounds.X, bounds.Y, size.X, font.LineHeight * text.GetLineCount() * fontSystem.FontResolutionFactor);
        }

        public override void Render(double deltaTime, RenderProperties properties)
        {
            var fontSystem = Window.FontSystems[this.font];
            var font = fontSystem.GetFont(fontSize);

            Window.FontRenderer.Begin(Transform.Matrix);
            if (segments == null)
            {
                font.DrawText(Window.FontRenderer, text, new Vector2(bounds.X, bounds.Y), color, 0, renderOffset, new Vector2(fontSystem.FontResolutionFactor, fontSystem.FontResolutionFactor));
            }
            else
            {
                font.DrawText(Window.FontRenderer, text, new Vector2(bounds.X, bounds.Y), TextSegmentColorizer.GetCharacterColors(text, color, segments), 0, renderOffset, new Vector2(fontSystem.FontResolutionFactor, fontSystem.FontResolutionFactor));
            }
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
            {
                var tuple = textFunc.Invoke();
                text = tuple.Item1;
                segments = tuple.Item2;
            }
        }
    }

    public struct TextSegment(Range range, UIColor color)
    {
        public Range Range = range;
        public UIColor Color = color;
    }

    public static class TextSegmentColorizer
    {
        public static FSColor[] GetCharacterColors(string text, UIColor defaultColor, TextSegment[] segments)
        {
            FSColor[] colors = new FSColor[text.Length];
            var defaultFS = (FSColor)defaultColor;
            Array.Fill(colors, defaultFS);

            foreach (var segment in segments)
            {
                (int Offset, int Length) = segment.Range.GetOffsetAndLength(colors.Length);
                Array.Fill(colors, segment.Color, Offset, Length);
            }

            return colors;
        }
    }
}
