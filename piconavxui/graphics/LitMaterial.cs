using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class LitMaterial : Material
    {
        public Texture? Diffuse { get; set; }
        public Vector3 DiffuseColor { get; set; }
        public Texture? Specular { get; set; }
        public Vector3 SpecularColor { get; set; }
        public float Smoothness { get; set; }
        public Texture? Emission { get; set; }
        public Vector3 EmissionColor { get; set; }
        public float Alpha { get; set; }

        public LitMaterial() : this("assets/shaders/vertex.glsl")
        {
        }

        public LitMaterial(string vertexPath) : base(new Shader(vertexPath, "assets/shaders/lit.glsl"))
        {
            Smoothness = 0.5f;
            Alpha = 1.0f;
            DiffuseColor = new Vector3(0.9f, 0.9f, 0.9f);
            SpecularColor = new Vector3(1, 1, 1);
            EmissionColor = new Vector3(0, 0, 0);
        }

        public override void Use(RenderProperties properties)
        {
            base.Use(properties);
            (Diffuse ?? Texture.White).Bind(TextureUnit.Texture0);
            (Specular ?? Texture.White).Bind(TextureUnit.Texture1);
            (Emission ?? Texture.White).Bind(TextureUnit.Texture2);

            Shader.SetUniform("viewPos", properties.Camera!.Position);
            Shader.SetUniform("material.diffuse", 0);
            Shader.SetUniform("material.diffuse_tint", DiffuseColor);
            Shader.SetUniform("material.specular", 1);
            Shader.SetUniform("material.specular_tint", SpecularColor);
            Shader.SetUniform("material.smoothness", Smoothness);
            Shader.SetUniform("material.emission", 2);
            Shader.SetUniform("material.emission_tint", EmissionColor);
            Shader.SetUniform("material.alpha", Alpha);

            Shader.SetUniform("light.ambient", properties.Light!.AmbientColor);
            Shader.SetUniform("light.color", properties.Light.Color);
            Shader.SetUniform("light.position", properties.Light.Transform.Position);
        }
    }
}
