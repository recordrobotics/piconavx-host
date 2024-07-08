using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class SensorMaterial : LitMaterial, IDisposable
    {
        public SensorMaterial() : base()
        {
            Diffuse = new Texture("assets/textures/navxmicro.png");
            DiffuseColor = new System.Numerics.Vector3(1, 1, 1);
        }

        public new void Dispose()
        {
            GC.SuppressFinalize(this);
            Diffuse?.Dispose();
            base.Dispose();
        }
    }
}
