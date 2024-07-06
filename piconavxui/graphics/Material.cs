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
        }

        public void Dispose()
        {
            Shader.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
