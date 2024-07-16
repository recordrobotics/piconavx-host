using piconavx.ui.graphics;
using piconavx.ui.graphics.ui;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.controllers
{
    public class AnchorLayout : Controller
    {
        public UIController Component { get; }

        /// <summary>
        /// The container to which to anchor to.
        /// Can be set to null to anchor to the frame buffer
        /// </summary>
        public UIController? Container { get; set; }

        public Anchor Anchor { get; set; } = Anchor.TopLeft;
        public Insets Insets { get; set; }
        public bool AllowResize { get; set; } = true;

        public AnchorLayout(UIController component):this(component, null)
        {}

        public AnchorLayout(UIController component, UIController? container)
        {
            Component = component;
            Container = container;
            RectangleF cont = Container?.Bounds ??
                new RectangleF(0, 0, Window.Current.Internal.FramebufferSize.X, Window.Current.Internal.FramebufferSize.Y);
            Insets = new Insets(component.Bounds.Left - cont.Left, component.Bounds.Top - cont.Top, cont.Right - component.Bounds.Right, cont.Bottom - component.Bounds.Bottom);
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
            RectangleF container = Container?.Bounds ??
                new RectangleF(0, 0, Window.Current.Internal.FramebufferSize.X, Window.Current.Internal.FramebufferSize.Y);

            if (Anchor.HasFlag(Anchor.Left) && Anchor.HasFlag(Anchor.Right)) // horizontal stretch
            {
                RectangleF target = new RectangleF(container.Left + Insets.Left, bounds.Y, container.Width - Insets.Horizontal, bounds.Height);

                if (AllowResize)
                {
                    bounds = target;
                } else
                {
                    bounds.X = target.X + target.Width / 2 - bounds.Width / 2;
                }
            }
            else if (Anchor.HasFlag(Anchor.Left))
            {
                bounds.X = container.Left + Insets.Left;
            }
            else if (Anchor.HasFlag(Anchor.Right))
            {
                bounds.X = container.Right - Insets.Right - bounds.Width;
            }

            if (Anchor.HasFlag(Anchor.Top) && Anchor.HasFlag(Anchor.Bottom)) // vertical stretch
            {
                RectangleF target = new RectangleF(bounds.X, container.Top + Insets.Top, bounds.Width, container.Height - Insets.Vertical);
                if (AllowResize)
                {
                    bounds = target;
                }
                else
                {
                    bounds.Y = target.Y + target.Height / 2 - bounds.Height / 2;
                }
            }
            else if (Anchor.HasFlag(Anchor.Top))
            {
                bounds.Y = container.Top + Insets.Top;
            }
            else if (Anchor.HasFlag(Anchor.Bottom))
            {
                bounds.Y = container.Bottom - Insets.Bottom - bounds.Height;
            }

            Component.Bounds = bounds;
        }
    }

    [Flags]
    public enum Anchor
    {
        None = 0,
        Top = 1 << 0,
        Left = 1 << 1,
        Right = 1 << 2,
        Bottom = 1 << 3,
        TopLeft = Top | Left,
        All = Top | Left | Right | Bottom
    }
}
