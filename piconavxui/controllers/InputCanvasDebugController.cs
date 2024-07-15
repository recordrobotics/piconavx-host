using piconavx.ui.graphics;
using piconavx.ui.graphics.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.controllers
{
    public class InputCanvasDebugController : Controller
    {
        /// <summary>
        /// Draws a thin yellow outline around the current <see cref="Canvas.Target"/> of the <see cref="Canvas.InputCanvas"/>
        /// </summary>
        public bool TargetBoundsOutline { get; set; } = false;
        /// <summary>
        /// Calculates the input raycast (<see cref="Canvas.RaycastAt(System.Numerics.Vector2)"/>) every frame for easier debugging in graphics debuggers
        /// </summary>
        public bool DebugRaycastGraphics { get; set; } = false;

        public override void Subscribe()
        {
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.AfterGeneral, Scene_Update);
            Scene.Render += new PrioritizedAction<RenderPriority, double, RenderProperties>(RenderPriority.UI, Scene_Render);
        }

        public override void Unsubscribe()
        {
            Scene.Update -= Scene_Update;
            Scene.Render -= Scene_Render;
        }

        private void Scene_Update(double deltaTime)
        {
            // Calculate raycast every frame (instead of only when necessary) for debugging in graphics debuggers
            if (DebugRaycastGraphics)
            {
                if (Canvas.InputCanvas != null)
                {
                    Canvas.InputCanvas.RaycastAt(Window.Current.Input!.Mice[0].Position);
                }
            }
        }

        private void Scene_Render(double deltaTime, RenderProperties properties)
        {
            if (Canvas.InputCanvas != null && Canvas.InputCanvas.Target != null)
            {
                // Draw outline around target bounds
                if (TargetBoundsOutline)
                {
                    UIMaterial.ColorMaterial.Use(properties);
                    Tessellator.Quad.DrawRectangleOutline(Canvas.InputCanvas.Target.Bounds, new SixLabors.ImageSharp.PixelFormats.Rgba32(255, 255, 0, 255), 1);
                    Tessellator.Quad.Flush();
                }
            }
        }
    }
}
