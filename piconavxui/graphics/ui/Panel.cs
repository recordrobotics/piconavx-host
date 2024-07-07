using System.Drawing;

namespace piconavx.ui.graphics.ui
{
    public class Panel : UIController
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

        private RectangleF bounds;
        public override RectangleF Bounds { get => bounds; set => bounds = value; }

        public Panel(Canvas canvas) : base(canvas)
        {
            bounds = new RectangleF(0, 0, 0, 0);
        }

        public override void Render(double deltaTime, RenderProperties properties)
        {
        }

        public override void Subscribe()
        {
        }

        public override void Unsubscribe()
        {
        }
    }
}
