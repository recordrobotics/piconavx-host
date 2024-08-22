using piconavx.ui.graphics;
using piconavx.ui.graphics.ui;
using Silk.NET.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.controllers
{
    public class OrbitCameraController : Controller
    {
        public float MouseSensitivity { get; set; } = 0.1f;
        public Vector3 Target { get; set; } = Vector3.Zero;
        public float Distance { get; set; } = 10.0f;

        public float Yaw { get => yaw; set => yaw = value; }
        public float Pitch { get => pitch; set => pitch = value; }

        private Camera camera;
        private float yaw = 0;
        private float pitch = 0;

        private bool movingCamera = false;

        public OrbitCameraController(Camera camera)
        {
            this.camera = camera;
        }

        public override void Subscribe()
        {
            Scene.MouseMove += new PrioritizedAction<GenericPriority, float, float, float, float>(GenericPriority.Medium, Scene_MouseMove);
            Scene.MouseDown += new PrioritizedAction<GenericPriority, MouseButton>(GenericPriority.Medium, Scene_MouseDown);
            Scene.MouseUp += new PrioritizedAction<GenericPriority, MouseButton>(GenericPriority.Medium, Scene_MouseUp);
            Scene.MouseScroll += new PrioritizedAction<GenericPriority, ScrollWheel>(GenericPriority.Medium, Scene_MouseScroll);
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
        }

        public override void Unsubscribe()
        {
            Scene.MouseMove -= Scene_MouseMove;
            Scene.MouseDown -= Scene_MouseDown;
            Scene.MouseUp -= Scene_MouseUp;
            Scene.MouseScroll -= Scene_MouseScroll;
            Scene.Update -= Scene_Update;
        }

        private void Scene_Update(double deltaTime)
        {
            Vector3 cameraPosition = new Vector3(
                MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch)) * Distance,
                MathF.Sin(MathHelper.DegreesToRadians(pitch)) * Distance,
                MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch)) * Distance
            );

            Vector3 cameraDirection = Vector3.Normalize(Target - cameraPosition);

            camera.Position = cameraPosition;
            camera.Front = cameraDirection;
        }

        private void Scene_MouseMove(float x, float y, float dx, float dy)
        {
            if (movingCamera)
            {
                yaw = (yaw + dx * MouseSensitivity) % 360.0f;
                pitch = MathF.Max(-89.0f, MathF.Min(89.0f, pitch + dy * MouseSensitivity));
            }
        }

        private void Scene_MouseDown(MouseButton button)
        {
            if (button == MouseButton.Left && Window.Current.Input != null && (Canvas.InputCanvas == null || Canvas.InputCanvas.Target == null))
            {
                foreach (var mouse in Window.Current.Input.Mice)
                {
                    mouse.Cursor.CursorMode = CursorMode.Raw;
                    movingCamera = true;
                }
            }
        }

        private void Scene_MouseUp(MouseButton button)
        {
            if (button == MouseButton.Left && Window.Current.Input != null)
            {
                foreach (var mouse in Window.Current.Input.Mice)
                {
                    mouse.Cursor.CursorMode = CursorMode.Normal;
                    movingCamera = false;
                }
            }
        }

        private void Scene_MouseScroll(ScrollWheel scroll)
        {
            if (Canvas.InputCanvas == null || Canvas.InputCanvas.RaycastAt(Window.Current.Input!.Mice[0].Position, Canvas.RaycastMode.Primary) == null) // use primary here because we only care about misses
            {
                Distance = Math.Clamp(Distance - scroll.Y * 0.3f, 1.0f, 20f);
            }
        }
    }
}
