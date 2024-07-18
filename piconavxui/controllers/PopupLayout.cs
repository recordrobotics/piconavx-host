using piconavx.ui.graphics;
using piconavx.ui.graphics.ui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.controllers
{
    public class PopupLayout : Controller
    {
        public UIController Component { get; }

        /// <summary>
        /// The component to which to position to.
        /// Can be set to null to position to the frame buffer
        /// </summary>
        public UIController? Target { get; set; }

        /// <summary>
        /// The container in which to position to.
        /// Can be set to null to position in the frame buffer
        /// </summary>
        public UIController? Container { get; set; }

        public Vector2 Offset { get; set; } = Vector2.Zero;

        public PopupAnchor Anchor { get; set; } = PopupAnchor.Center;

        public PopupLayout(UIController component) : this(component, null)
        { }

        public PopupLayout(UIController component, UIController? target) : this(component, target, null)
        { }

        public PopupLayout(UIController component, UIController? target, UIController? container)
        {
            Component = component;
            Target = target;
            Container = container;
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
            RectangleF bounds = Component.Bounds;
            RectangleF target = Target?.Bounds ??
                new RectangleF(0, 0, Window.Current.Internal.FramebufferSize.X, Window.Current.Internal.FramebufferSize.Y);
            RectangleF container = Container?.Bounds ??
                new RectangleF(0, 0, Window.Current.Internal.FramebufferSize.X, Window.Current.Internal.FramebufferSize.Y);

            if (Target == null)
            {
                switch (Anchor)
                {
                    case PopupAnchor.Center:
                        bounds.X = target.X + target.Width / 2 - bounds.Width / 2 + Offset.X;
                        bounds.Y = target.Y + target.Height / 2 - bounds.Height / 2 + Offset.Y;
                        break;
                    case PopupAnchor.TopLeft:
                        bounds.X = target.X + Offset.X;
                        bounds.Y = target.Y + Offset.Y;
                        break;
                    case PopupAnchor.Top:
                        bounds.X = target.X + target.Width / 2 - bounds.Width / 2 + Offset.X;
                        bounds.Y = target.Y + Offset.Y;
                        break;
                    case PopupAnchor.TopRight:
                        bounds.X = target.Right - bounds.Width - Offset.X;
                        bounds.Y = target.Y + Offset.Y;
                        break;
                    case PopupAnchor.BottomLeft:
                        bounds.X = target.X + Offset.X;
                        bounds.Y = target.Bottom - bounds.Height - Offset.Y;
                        break;
                    case PopupAnchor.Bottom:
                        bounds.X = target.X + target.Width / 2 - bounds.Width / 2 + Offset.X;
                        bounds.Y = target.Bottom - bounds.Height - Offset.Y;
                        break;
                    case PopupAnchor.BottomRight:
                        bounds.X = target.Right - bounds.Width - Offset.X;
                        bounds.Y = target.Bottom - bounds.Height - Offset.Y;
                        break;
                    case PopupAnchor.Left:
                        bounds.X = target.X + Offset.X;
                        bounds.Y = target.Y + target.Height / 2 - bounds.Height / 2 + Offset.Y;
                        break;
                    case PopupAnchor.Right:
                        bounds.X = target.Right - bounds.Width - Offset.X;
                        bounds.Y = target.Y + target.Height / 2 - bounds.Height / 2 + Offset.Y;
                        break;
                }
            }
            else
            {
                switch (Anchor)
                {
                    case PopupAnchor.Center:
                        bounds.X = target.X + target.Width / 2 - bounds.Width / 2 + Offset.X;
                        bounds.Y = target.Y + target.Height / 2 - bounds.Height / 2 + Offset.Y;
                        break;
                    case PopupAnchor.TopLeft:
                        bounds.X = target.Left - bounds.Width - Offset.Y;
                        bounds.Y = target.Top - bounds.Height - Offset.X;
                        break;
                    case PopupAnchor.Top:
                        bounds.X = target.X + target.Width / 2 - bounds.Width / 2 + Offset.Y;
                        bounds.Y = target.Top - bounds.Height - Offset.X;
                        break;
                    case PopupAnchor.TopRight:
                        bounds.X = target.Right + Offset.Y;
                        bounds.Y = target.Top - bounds.Height - Offset.X;
                        break;
                    case PopupAnchor.BottomLeft:
                        bounds.X = target.Left - bounds.Width - Offset.Y;
                        bounds.Y = target.Bottom + Offset.X;
                        break;
                    case PopupAnchor.Bottom:
                        bounds.X = target.X + target.Width / 2 - bounds.Width / 2 + Offset.Y;
                        bounds.Y = target.Bottom + Offset.X;
                        break;
                    case PopupAnchor.BottomRight:
                        bounds.X = target.Right + Offset.Y;
                        bounds.Y = target.Bottom + Offset.X;
                        break;
                    case PopupAnchor.Left:
                        bounds.X = target.Left - bounds.Width - Offset.X;
                        bounds.Y = target.Y + target.Height / 2 - bounds.Height / 2 + Offset.Y;
                        break;
                    case PopupAnchor.Right:
                        bounds.X = target.Right + Offset.X;
                        bounds.Y = target.Y + target.Height / 2 - bounds.Height / 2 + Offset.Y;
                        break;
                }
            }

            // position within the container bounds
            if (bounds.Left < container.Left)
                bounds.X = container.Left;
            else if (bounds.Right > container.Right)
                bounds.X = container.Right - bounds.Width;

            if (bounds.Top < container.Top)
                bounds.Y = container.Top;
            else if (bounds.Bottom > container.Bottom)
                bounds.Y = container.Bottom - bounds.Height;

            Component.Bounds = bounds;
        }
    }

    public enum PopupAnchor
    {
        Center,
        TopLeft,
        Top,
        TopRight,
        BottomLeft,
        Bottom,
        BottomRight,
        Left,
        Right
    }
}
