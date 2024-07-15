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
        public bool TargetBoundsOutline { get; set; } = true;

        public override void Subscribe()
        {
            Scene.Render += new PrioritizedAction<RenderPriority, double, RenderProperties>(RenderPriority.UI, Scene_Render);
        }

        public override void Unsubscribe()
        {
            Scene.Render -= Scene_Render;
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
