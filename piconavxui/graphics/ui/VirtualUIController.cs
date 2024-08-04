using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public class VirtualUIController : UIController
    {
        public VirtualUIController(Canvas canvas) : base(canvas)
        {
        }

        public Func<int>? GetZIndex { get; set; }
        public Action<int>? SetZIndex { get; set; }

        private int zIndex = 0;
        public override int ZIndex
        {
            get => GetZIndex?.Invoke() ?? zIndex; set
            {
                if (SetZIndex == null)
                {
                    zIndex = value;
                }
                else
                {
                    SetZIndex(value);
                }
            }
        }

        public Func<RectangleF>? GetBounds { get; set; }
        public Action<RectangleF>? SetBounds { get; set; }

        private RectangleF bounds;
        public override RectangleF Bounds
        {
            get => GetBounds?.Invoke() ?? bounds; set
            {
                if (SetBounds == null)
                {
                    bounds = value;
                }
                else
                {
                    SetBounds(value);
                }
            }
        }

        public override void Subscribe()
        {
            throw new NotImplementedException();
        }

        public override void Unsubscribe()
        {
            throw new NotImplementedException();
        }
    }
}
