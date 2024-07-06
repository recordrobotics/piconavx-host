using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class Material : IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static Material DefaultMaterial;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static Material CreateDefault()
        {
            return new Material(new Shader("assets/shaders/vertex.glsl", "assets/shaders/default.glsl"));
        }

        protected Shader Shader { get; }

        public bool EnableDepthTest { get; set; } = true;
        public bool EnableBlend { get; set; } = false;

        public Material(Shader shader)
        {
            this.Shader = shader;
        }

        public virtual void Use(RenderProperties properties)
        {
            Shader.Use();
            Shader.SetUniform("uModel", properties.Transform!.Matrix);
            Shader.SetUniform("uView", properties.Camera!.GetViewMatrix());
            Shader.SetUniform("uProjection", properties.Camera.GetProjectionMatrix());

            if (EnableDepthTest)
                Window.GL.Enable(EnableCap.DepthTest);
            else
                Window.GL.Disable(EnableCap.DepthTest);

            if (EnableBlend)
            {
                Window.GL.Enable(EnableCap.Blend);
                Window.GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
            }
            else
                Window.GL.Disable(EnableCap.Blend);
        }

        public void Dispose()
        {
            Shader.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
