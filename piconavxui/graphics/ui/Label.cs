using FontStashSharp;
using Silk.NET.Assimp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
            bounds = new RectangleF(0, 0, 50, 10);
        }

        public override void Render(double deltaTime, RenderProperties properties)
        {
            var font = Window.FontSystem.GetFont(12);
            Window.FontRenderer.Begin();
            font.DrawText(Window.FontRenderer, text, new Vector2(bounds.X, bounds.Y), color, 0, default, new Vector2(Window.FontSystem.FontResolutionFactor, Window.FontSystem.FontResolutionFactor),0,5);
            Window.FontRenderer.End();
        }

        public override void Subscribe()
        {

        }

        public override void Unsubscribe()
        {

        }
    }
}
