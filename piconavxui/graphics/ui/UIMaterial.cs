using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public class UIMaterial : Material, IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static new UIMaterial DefaultMaterial;
        public static UIMaterial ColorMaterial;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static new UIMaterial CreateDefault()
        {
            return new UIMaterial(new Shader("assets/shaders/uivertex.glsl", "assets/shaders/default.glsl"), null);
        }

        public static UIMaterial CreateColorMaterial()
        {
            return new UIMaterial(new Shader("assets/shaders/uivertex.glsl", "assets/shaders/color.glsl"), null);
        }

        protected Shader? HitTestShader { get; }

        public UIMaterial(Shader shader, Shader? hitTestShader) : base(shader)
        {
            HitTestShader = hitTestShader;
        }

        public override void Use(RenderProperties properties)
        {
            Use(properties, null);
        }

        public virtual void Use(RenderProperties properties, Transform? transform)
        {
            Window.GL.Disable(EnableCap.DepthTest);
            Window.GL.Enable(EnableCap.Blend);
            Window.GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Shader.Use();
            Shader.SetUniform("uMatrix", properties.Canvas!.Matrix);
            Shader.SetUniform("tMatrix", transform?.Matrix ?? Matrix4x4.Identity);
        }

        public virtual void UseHitTest(Canvas canvas, byte id, Transform? transform)
        {
            if (HitTestShader != null)
            {
                HitTestShader.Use();
                HitTestShader.SetUniform("uMatrix", canvas.Matrix);
                HitTestShader.SetUniform("tMatrix", transform?.Matrix ?? Matrix4x4.Identity);
                HitTestShader.SetUniform("uHitID", (uint)id);
            }
        }

        public override void UpdateModelBuffer(RenderProperties properties)
        {
        }

        public override void UpdateInstanceBuffer(RenderProperties properties)
        {
        }

        public new void Dispose()
        {
            base.Dispose();
            HitTestShader?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
