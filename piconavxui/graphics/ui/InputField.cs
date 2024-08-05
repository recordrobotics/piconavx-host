using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

        public override void Subscribe()
        {
            base.Subscribe();
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
        }

        protected virtual void UpdateZIndex() { }
    }
}
