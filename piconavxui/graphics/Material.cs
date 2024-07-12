using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class Material : IDisposable
    {
        public const int MAX_INSTANCE_COUNT = 1024;

        [StructLayout(LayoutKind.Sequential)]
        struct MatrixBlock
        {
            public Matrix4x4 uModel;
        }

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct MatrixBlockInstanced
        {
            public fixed float uModel[MAX_INSTANCE_COUNT * 16];
        }

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
        public bool UseInstanced { get; set; } = false;
        public bool ExtendedDrawCall { get; set; } = false;

        private static MatrixBlock matrixBlock = new();
        private static MatrixBlockInstanced matrixBlockInstanced = new();
        private static BufferObject<MatrixBlock>? matrixBlockBuffer;
        private static BufferObject<MatrixBlockInstanced>? matrixBlockInstancedBuffer;

        public static void CreateStaticResources()
        {
            matrixBlockBuffer = new BufferObject<MatrixBlock>(matrixBlock, BufferTargetARB.UniformBuffer, true);
            matrixBlockInstancedBuffer = new BufferObject<MatrixBlockInstanced>(matrixBlockInstanced, BufferTargetARB.UniformBuffer, true);
            Scene.AddResource(matrixBlockBuffer);
            Scene.AddResource(matrixBlockInstancedBuffer);
        }

        public Material(Shader shader)
        {
            this.Shader = shader;
        }

        public virtual void Use(RenderProperties properties)
        {
            Shader.Use();

            if (UseInstanced)
            {
                matrixBlockInstancedBuffer?.BindBufferBase(0);
            }
            else
            {
                matrixBlockBuffer?.BindBufferBase(0);
            }

            Shader.SetUniformBlock("MatrixBlock", 0);
            Shader.SetUniform("uView", properties.Camera!.GetViewMatrix());
            Shader.SetUniform("uProjection", properties.Camera.GetProjectionMatrix());

            if (EnableDepthTest)
                Window.GL.Enable(EnableCap.DepthTest);
            else
                Window.GL.Disable(EnableCap.DepthTest);

            if (EnableBlend)
            {
                Window.GL.Enable(EnableCap.Blend);
                Window.GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            }
            else
                Window.GL.Disable(EnableCap.Blend);
        }

        public virtual void UpdateModelBuffer(RenderProperties properties)
        {
            matrixBlock.uModel = properties.Transforms![0]!.Matrix;
            matrixBlockBuffer?.SetData(matrixBlock);
        }

        public virtual void UpdateInstanceBuffer(RenderProperties properties)
        {
            int len = properties.Transforms!.Length;
            if (len > MAX_INSTANCE_COUNT)
            {
                Debug.WriteLine("WARNING: Attempted to draw " + len + " instances, while only a maximum of " + MAX_INSTANCE_COUNT + " is supported.");
                len = MAX_INSTANCE_COUNT;
            }
            Matrix4x4[] matrices = new Matrix4x4[len];
            for (int i = 0; i < len; i++)
            {
                matrices[i] = properties.Transforms[i]?.Matrix ?? Matrix4x4.Identity;
            }

            unsafe
            {
                fixed (float* ptr = matrixBlockInstanced.uModel)
                {
                    Matrix4x4* mat = (Matrix4x4*)ptr;
                    for (int i = 0; i < matrices.Length; i++)
                    {
                        mat[i] = matrices[i];
                    }
                }
            }
            matrixBlockInstancedBuffer?.SetData(matrixBlockInstanced);
        }

        public void Dispose()
        {
            Shader.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
