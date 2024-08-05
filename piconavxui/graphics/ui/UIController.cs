using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
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
        private bool _secondaryInputVisible = false;
        public virtual bool SecondaryInputVisible { get => _secondaryInputVisible; set => _secondaryInputVisible = value; }
        public virtual bool IsRenderable { get => true; }

        public static Vector2 GlobalScale { get => new(Transform.GlobalScale.X, Transform.GlobalScale.Y); set => Transform.GlobalScale = new(value, 1); }

        static UIController()
        {
            GlobalScale = new Vector2(1f / 1.5f);
        }

        private Transform _transform;
        public virtual Transform Transform { get => _transform; set => _transform = value; }

        public virtual bool MouseOver { get; set; } = false;
        public virtual bool MouseDown { get; set; } = false;

        public PrioritizedList<PrioritizedAction<GenericPriority>> Click = new();
        public PrioritizedList<PrioritizedAction<GenericPriority, Vector2>> Scroll = new();

        protected UIController(Canvas canvas)
        {
            Canvas = canvas;
            _transform = new Transform()
            {
                UseGlobalScale = true
            };
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
        public void NotifyScroll(Vector2 scroll)
        {
            foreach (var action in Scroll)
            {
                action.Action.Invoke(scroll);
            }
        }
        public virtual void OnAdd() { }
        public virtual void OnRemove() { }
    }
}
