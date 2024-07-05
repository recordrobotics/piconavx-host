using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class CatMaterial : LitMaterial, IDisposable
    {
        public CatMaterial() : base()
        {
            Diffuse = new Texture("assets/textures/diffuse.png");
            Specular = new Texture("assets/textures/specular.png");
        }

        public new void Dispose()
        {
            GC.SuppressFinalize(this);
            Diffuse?.Dispose();
            Specular?.Dispose();
            base.Dispose();
        }
    }
}
