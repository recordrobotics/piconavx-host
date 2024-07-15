using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public abstract class UIController : Controller, IComparable<UIController>
    {
        public Canvas Canvas { get; }
        public abstract int ZIndex { get; set; }
        public abstract RectangleF Bounds { get; set; }

        private RaycastTransparency _raycastTransparency = RaycastTransparency.Opaque;
        public virtual RaycastTransparency RaycastTransparency { get => _raycastTransparency; set => _raycastTransparency = value; }
        public virtual bool IsRenderable { get => true; }

        public virtual bool MouseOver { get; set; } = false;
        public virtual bool MouseDown { get; set; } = false;

        public PrioritizedList<PrioritizedAction<GenericPriority>> Click = new();

        protected UIController(Canvas canvas)
        {
            Canvas = canvas;
        }

        public int CompareTo(UIController? other)
        {
            return ZIndex.CompareTo(other?.ZIndex);
        }

        public virtual void Render(double deltaTime, RenderProperties properties) { }
        public virtual void HitTest(byte id) { }
        public void NotifyClick()
        {
            foreach (var action in Click)
            {
                action.Action.Invoke();
            }
        }
    }
}
