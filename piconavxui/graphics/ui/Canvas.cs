using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public class Canvas : Controller
    {
        private List<UIController> components;

        public IReadOnlyList<UIController> Components { get { return components.AsReadOnly(); } }

        public Matrix4x4 Matrix { get; private set; }

        public Canvas()
        {
            components = new List<UIController>();
        }

        public void AddComponent(UIController component)
        {
            components.Add(component);
            InvalidateHierarchy();
        }

        public void RemoveComponent(UIController component)
        {
            components.Remove(component);
        }

        public void InvalidateHierarchy()
        {
            components.Sort();
        }

        public override void Subscribe()
        {
            Scene.Render += new PrioritizedAction<RenderPriority, double, RenderProperties>(RenderPriority.UI, Scene_Render);
        }

        public override void Unsubscribe()
        {
            Scene.Render -= Scene_Render;
        }

        private void Scene_Render(double deltaTime, RenderProperties properties)
        {
            properties.Canvas = this;
            Matrix = Matrix4x4.CreateOrthographicOffCenter(0, Window.Current.Internal.FramebufferSize.X, Window.Current.Internal.FramebufferSize.Y, 0, 0, -1);

            // Render components in Z-index order
            foreach (UIController component in components)
            {
                component.Render(deltaTime, properties);
            }
        }
    }
}
