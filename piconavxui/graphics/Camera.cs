using piconavx.ui.graphics.ui;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public class Camera : Controller
    {
        public Vector3 Position { get; set; }
        public Vector3 Front { get; set; }
        public float Fov { get; set; } = 70f;

        public Vector3 Up { get; set; }
        public float AspectRatio { get; private set; }

        public Camera(Vector3 position, Vector3 front, Vector3 up)
        {
            Position = position;
            AspectRatio = (float)Window.Current.Internal.FramebufferSize.X / Window.Current.Internal.FramebufferSize.Y;
            Front = front;
            Up = up;
        }

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.CreateLookAt(Position, Position + Front, Up);
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov), AspectRatio, 0.1f, 100.0f);
        }

        public override void Subscribe()
        {
            Scene.ViewportChange += new PrioritizedAction<GenericPriority, Silk.NET.Maths.Rectangle<int>>(GenericPriority.Highest, Scene_ViewportChange);
            Scene.Render += new PrioritizedAction<RenderPriority, double, RenderProperties>(RenderPriority.RestoreContext, Render);
        }

        public override void Unsubscribe()
        {
            Scene.ViewportChange -= Scene_ViewportChange;
            Scene.Render -= Render;
        }

        private void Scene_ViewportChange(Silk.NET.Maths.Rectangle<int> viewport)
        {
            AspectRatio = (float)viewport.Size.X / viewport.Size.Y;
        }

        private void Render(double deltaTime, RenderProperties properties)
        {
            Window.GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            Window.GL.Viewport(Window.Current.Internal.FramebufferSize);
            var color = Theme.Viewport.Value.ToVector4();
            Window.GL.ClearColor(color.X, color.Y, color.Z, color.W);
            Window.GL.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
            properties.Camera = this;
        }
    }
}
