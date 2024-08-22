using piconavx.ui.graphics.ui;
using piconavx.ui.graphics;
using System.Drawing;

namespace piconavx.ui.controllers
{
    public class FillLayout : Controller
    {
        public UIController Component { get; }

        /// <summary>
        /// The container to which to anchor to.
        /// Can be set to null to anchor to the frame buffer
        /// </summary>
        public UIController? Container { get; set; }

        public bool Horizontal { get; set; } = false;
        public bool Vertical { get; set; } = false;

        public FillLayout(UIController component) : this(component, null)
        { }

        public FillLayout(UIController component, UIController? container)
        {
            Component = component;
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
            RectangleF container = Container?.Bounds ??
                new RectangleF(0, 0, Window.Current.FramebufferSize.X, Window.Current.FramebufferSize.Y);

            if (Horizontal)
            {
                bounds.Width = Math.Max(0, container.Right - bounds.Left);
            }

            if (Vertical)
            {
                bounds.Height = Math.Max(0, container.Bottom - bounds.Top);
            }

            Component.Bounds = bounds;
        }
    }
}
