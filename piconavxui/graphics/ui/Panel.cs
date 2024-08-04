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
                UpdateZIndex();
                Canvas.InvalidateHierarchy();
            }
        }

        private RectangleF bounds;
        public override RectangleF Bounds { get => bounds; set => bounds = value; }
        public override bool IsRenderable => supportsInputEvents;

        public bool SupportsInputEvents { get => supportsInputEvents; set => supportsInputEvents = value; }
        private bool supportsInputEvents = false;

        public Panel(Canvas canvas) : base(canvas)
        {
            bounds = new RectangleF(0, 0, 0, 0);
        }

        public override void Subscribe()
        {
        }

        public override void Unsubscribe()
        {
        }

        protected virtual void UpdateZIndex() { }
    }
}
