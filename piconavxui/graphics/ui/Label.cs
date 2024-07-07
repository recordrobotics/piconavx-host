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

        private Func<string>? textFunc;
        public Func<string>? TextDelegate { get => textFunc; set => textFunc = value; }

        private float fontSize = 12.0f;
        public float FontSize
        {
            get => fontSize; set => fontSize = value;
        }

        private float CharSpacing => fontSize / 12.0f * 5.0f;

        private bool autoSize = true;
        public bool AutoSize { get => autoSize; set => autoSize = value; }

        private RectangleF bounds;
        public override RectangleF Bounds { get => bounds; set => bounds = value; }

        private FSColor color;
        public FSColor Color
        {
            get => color; set => color = value;
        }

        public Label(string text, Canvas canvas) : base(canvas)
        {
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
            var font = Window.FontSystem.GetFont(fontSize);
            Vector2 size = font.MeasureString(text, new Vector2(Window.FontSystem.FontResolutionFactor, Window.FontSystem.FontResolutionFactor), CharSpacing);
            return new RectangleF(bounds.X, bounds.Y, size.X, size.Y);
        }

        public override void Render(double deltaTime, RenderProperties properties)
        {
            var font = Window.FontSystem.GetFont(fontSize);
            Window.FontRenderer.Begin();
            font.DrawText(Window.FontRenderer, text, new Vector2(bounds.X, bounds.Y), color, 0, default, new Vector2(Window.FontSystem.FontResolutionFactor, Window.FontSystem.FontResolutionFactor), 0, CharSpacing);
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
