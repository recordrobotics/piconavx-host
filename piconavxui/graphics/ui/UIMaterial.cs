using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public class UIMaterial : Material
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static new UIMaterial DefaultMaterial;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static new UIMaterial CreateDefault()
        {
            return new UIMaterial(new Shader("assets/shaders/uivertex.glsl", "assets/shaders/default.glsl"));
        }

        public UIMaterial(Shader shader) : base(shader)
        {
        }

        public override void Use(RenderProperties properties)
        {
            Window.GL.Disable(EnableCap.DepthTest);
            Window.GL.Enable(EnableCap.Blend);
            Window.GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Shader.Use();
            Shader.SetUniform("uMatrix", properties.Canvas!.Matrix);
        }

        public override void UpdateModelBuffer(RenderProperties properties)
        {
        }

        public override void UpdateInstanceBuffer(RenderProperties properties)
        {
        }
    }
}
