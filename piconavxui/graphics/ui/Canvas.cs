using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public class Canvas : Controller, IDisposable
    {
        private List<UIController> components;
        private Framebuffer raycastFrameBuffer;

        public static Canvas? InputCanvas { get; set; } = null;

        public IReadOnlyList<UIController> Components { get { return components.AsReadOnly(); } }

        public Matrix4x4 Matrix { get; private set; }

        private UIController? target = null;
        public UIController? Target { get => target; }

        public Canvas()
        {
            if (InputCanvas == null)
                InputCanvas = this;

            components = new List<UIController>();
            raycastFrameBuffer = new Framebuffer(
                (uint)Window.Current.Internal.FramebufferSize.X,
                (uint)Window.Current.Internal.FramebufferSize.Y,
                InternalFormat.R8ui, PixelFormat.RedInteger, PixelType.UnsignedByte);
        }

        public void AddComponent(UIController component)
        {
            component.OnAdd();
            components.Add(component);
            InvalidateHierarchy();
        }

        public void RemoveComponent(UIController component)
        {
            component.OnRemove();
            components.Remove(component);
        }

        public void InvalidateHierarchy()
        {
            components.Sort();
        }

        private Matrix4x4 CreateMatrix()
        {
            return Matrix4x4.CreateOrthographicOffCenter(0, Window.Current.Internal.FramebufferSize.X, Window.Current.Internal.FramebufferSize.Y, 0, 0, -1);
        }

        public enum RaycastMode
        {
            Primary,
            Secondary
        }

        private List<(int index, UIController controller)> opaqueMatches = new List<(int index, UIController controller)>();
        private List<(int index, UIController controller)> transparentMatches = new List<(int index, UIController controller)>();
        public UIController? RaycastAt(Vector2 point, RaycastMode mode)
        {
            opaqueMatches.Clear();
            transparentMatches.Clear();

            // Compile a list of all components whose bounding box contains the target point (sorted top to bottom)
            for (int i = components.Count - 1; i >= 0; i--)
            {
                var component = components[i];
                if (component.RaycastTransparency != RaycastTransparency.Hidden && component.IsRenderable && (mode == RaycastMode.Primary || component.SecondaryInputVisible) && component.Bounds.Transform(component.Transform.Matrix).Contains(point.X, point.Y))
                {
                    if (component.RaycastTransparency == RaycastTransparency.Transparent && transparentMatches.Count < 255) // 0 can't be used as element index since that is the clear value
                        transparentMatches.Add((i, component));
                    else // Fall back to opaque raycast after the first 255 transparent components
                        opaqueMatches.Add((i, component));
                }
            }

            // No AABB matches at all
            if (opaqueMatches.Count == 0 && transparentMatches.Count == 0)
                return null;
            // There is an opaque component on top of the highest transparent component
            else if (opaqueMatches.Count > 0 && (transparentMatches.Count == 0 || opaqueMatches[0].index > transparentMatches[0].index))
                return opaqueMatches[0].controller;

            // Since the highest component is transparent, perform a draw call to determine which component is hit
            raycastFrameBuffer.Bind();
            Window.GL.Viewport(new Size((int)raycastFrameBuffer.Width, (int)raycastFrameBuffer.Height));
            Window.GL.ClearColor(Color.FromArgb(0, 0, 0, 0));
            Window.GL.Clear((uint)ClearBufferMask.ColorBufferBit);
            Window.GL.Disable(EnableCap.DepthTest);
            Window.GL.Disable(EnableCap.Blend);

            Matrix = CreateMatrix();

            for (int i = transparentMatches.Count - 1; i >= 0; i--) // Reversed to draw from bottom to top
            {
                transparentMatches[i].controller.HitTest((byte)(i + 1)); // 0 is no hit
            }

            byte hitId = Window.GL.ReadPixels<byte>((int)point.X, (int)(raycastFrameBuffer.Height - point.Y), 1, 1, PixelFormat.RedInteger, PixelType.UnsignedByte);

            if (hitId == 0) // No transparent components were hit, return top opaque component instead
                return opaqueMatches.Count > 0 ? opaqueMatches[0].controller : null;

            var transparentHit = transparentMatches[hitId - 1];

            // Check if the top opaque component is on top of the transparent hit
            if (opaqueMatches.Count > 0 && opaqueMatches[0].index > transparentHit.index)
                return opaqueMatches[0].controller;
            else
                return transparentHit.controller;
        }

        public override void Subscribe()
        {
            Scene.Render += new PrioritizedAction<RenderPriority, double, RenderProperties>(RenderPriority.UI, Scene_Render);
            Scene.ViewportChange += new PrioritizedAction<GenericPriority, Silk.NET.Maths.Rectangle<int>>(GenericPriority.Highest, Scene_ViewportChange);
            Scene.MouseMove += new PrioritizedAction<GenericPriority, float, float, float, float>(GenericPriority.Highest, Scene_MouseMove);
            Scene.MouseDown += new PrioritizedAction<GenericPriority, Silk.NET.Input.MouseButton>(GenericPriority.Highest, Scene_MouseDown);
            Scene.MouseUp += new PrioritizedAction<GenericPriority, Silk.NET.Input.MouseButton>(GenericPriority.Highest, Scene_MouseUp);
            Scene.MouseScroll += new PrioritizedAction<GenericPriority, Silk.NET.Input.ScrollWheel>(GenericPriority.Highest, Scene_MouseScroll);
        }

        public override void Unsubscribe()
        {
            Scene.Render -= Scene_Render;
            Scene.ViewportChange -= Scene_ViewportChange;
        }

        public void InvalidateInput()
        {
            var mouse = Window.Current.Input!.Mice[0];
            Scene_MouseMove(mouse.Position.X, mouse.Position.Y, 0, 0);
        }

        private void Scene_MouseMove(float x, float y, float dx, float dy)
        {
            target = RaycastAt(new Vector2(x, y), RaycastMode.Primary);
            foreach(var component in components)
            {
                if(component.IsRenderable)
                    component.MouseOver = component == target;
            }
        }

        private void Scene_MouseDown(Silk.NET.Input.MouseButton button)
        {
            target = RaycastAt(Window.Current.Input!.Mice[0].Position, RaycastMode.Primary);
            if (button == Silk.NET.Input.MouseButton.Left)
            {
                foreach (var component in components)
                {
                    if (component.IsRenderable)
                        component.MouseDown = component == target;
                }
            }
        }

        private void Scene_MouseUp(Silk.NET.Input.MouseButton button)
        {
            if (button == Silk.NET.Input.MouseButton.Left)
            {
                foreach (var component in components)
                {
                    if (component.IsRenderable)
                    {
                        if (component.MouseDown && component.MouseOver)
                        {
                            component.NotifyClick();
                        }

                        component.MouseDown = false;
                    }
                }
            }
        }

        private void Scene_MouseScroll(Silk.NET.Input.ScrollWheel scrollWheel)
        {
            target = RaycastAt(Window.Current.Input!.Mice[0].Position, RaycastMode.Secondary);
            Vector2 scroll = new(scrollWheel.X, scrollWheel.Y);

            foreach (var component in components)
            {
                if (component.IsRenderable && component.SecondaryInputVisible && component == target)
                    component.NotifyScroll(scroll);
            }
        }

        private void Scene_Render(double deltaTime, RenderProperties properties)
        {
            properties.Canvas = this;
            Matrix = CreateMatrix();

            // Render components in Z-index order
            foreach (UIController component in components)
            {
                if (component.IsRenderable)
                    component.Render(deltaTime, properties);
            }
        }

        private void Scene_ViewportChange(Silk.NET.Maths.Rectangle<int> viewport)
        {
            raycastFrameBuffer.SetSize((uint)viewport.Size.X, (uint)viewport.Size.Y);
        }

        public void Dispose()
        {
            raycastFrameBuffer.Dispose();
        }
    }

    public enum RaycastTransparency
    {
        Opaque,
        Transparent,
        Hidden
    }
}
