using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class SensorFeedMaterial : LitMaterial, IDisposable
    {
        public SensorFeedMaterial() : base("assets/shaders/vertexinst.glsl")
        {
            Diffuse = new Texture("assets/textures/navxmicro.png");
            DiffuseColor = new System.Numerics.Vector3(1, 1, 1);
            UseInstanced = true;
            Alpha = 0.2f;
            EnableBlend = true;
            ExtendedDrawCall = true;
        }

        public new void Dispose()
        {
            GC.SuppressFinalize(this);
            Diffuse?.Dispose();
            base.Dispose();
        }
    }
}
