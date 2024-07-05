using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class Light : Controller
    {
        public Vector3 AmbientColor;
        public Vector3 Color;
        public Transform Transform;

        public Light(Vector3 ambientColor, Vector3 color, Transform transform)
        {
            AmbientColor = ambientColor;
            Color = color;
            Transform = transform;
        }

        public override void Subscribe()
        {
            Scene.Render += new PrioritizedAction<RenderPriority, double, RenderProperties>(RenderPriority.SetupContext, Render);
        }

        public override void Unsubscribe()
        {
            Scene.Render -= Render;
        }

        private void Render(double deltaTime, RenderProperties properties)
        {
            properties.Light = this;
        }
    }
}
