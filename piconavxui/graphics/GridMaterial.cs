using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class GridMaterial : Material
    {
        public Vector4 GridColor { get; set; }

        public GridMaterial() : base(new Shader("assets/shaders/vertex.glsl", "assets/shaders/grid.glsl"))
        {
            EnableBlend = true;
            GridColor = new Vector4(0.55f, 0.55f, 0.55f, 1.0f);
        }

        public override void Use(RenderProperties properties)
        {
            base.Use(properties);
            Shader.SetUniform("gridColor", GridColor);
        }
    }
}
