using piconavx.ui.graphics;
using piconavx.ui.graphics.ui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.controllers
{
    public class InputCanvasDebugController : Controller
    {
        /// <summary>
        /// Draws a thin blue outline showing the bounds of every renderable (<see cref="UIController.IsRenderable"/>) component
        /// </summary>
        public bool ShowBounds { get; set; } = false;
        /// <summary>
        /// When true, draws bounds on all components, even if <see cref="UIController.IsRenderable"/> = false. (<see cref="ShowBounds"/>)
        /// </summary>
        public bool ShowNonRenderableBounds { get; set; } = false;
        /// <summary>
        /// Draws a thin yellow outline around the current <see cref="Canvas.Target"/> of the <see cref="Canvas.InputCanvas"/>
        /// </summary>
        public bool TargetBoundsOutline { get; set; } = false;
        /// <summary>
        /// Draws a thin red outline around the current <see cref="Canvas.LastTarget"/> of the <see cref="Canvas.InputCanvas"/>
        /// </summary>
        public bool LastTargetBoundsOutline { get; set; } = false;
        /// <summary>
        /// Draws a thin cyan outline around the <see cref="FlowLayout.ContentBounds"/> of every <see cref="FlowLayout"/> in <see cref="FlowLayout.Instances"/>
        /// </summary>
        public bool FlowLayoutContentBoundsOutline { get; set; } = false;
        /// <summary>
        /// Calculates the input raycast (<see cref="Canvas.RaycastAt(System.Numerics.Vector2)"/>) every frame for easier debugging in graphics debuggers
        /// </summary>
        public bool DebugRaycastGraphics { get; set; } = false;
        /// <summary>
        /// Draws a semi-transparent overlay over every component where <see cref="UIController.MouseOver"/> = true
        /// </summary>
        public bool HighlightMouseOver { get; set; } = false;
        /// <summary>
        /// Draws a semi-transparent overlay over every component where <see cref="UIController.MouseDown"/> = true
        /// </summary>
        public bool HighlightMouseDown { get; set; } = false;
        /// <summary>
        /// When true, performs highlighting on all components, even if <see cref="UIController.IsRenderable"/> = false
        /// </summary>
        public bool HighlightNonRenderable { get; set; } = false;

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
                    Canvas.InputCanvas.RaycastAt(Window.Current.Input!.Mice[0].Position, Canvas.RaycastMode.Primary);
                }
            }
        }

        private void Scene_Render(double deltaTime, RenderProperties properties)
        {
            if (Canvas.InputCanvas != null)
            {
                UIMaterial.ColorMaterial.Use(properties);

                // Highlight MouseDown and MouseOver
                if (HighlightMouseDown || HighlightMouseOver || ShowBounds)
                {
                    foreach (var component in Canvas.InputCanvas.Components)
                    {
                        var bounds = component.Bounds.Transform(component.Transform.Matrix);
                        if (ShowBounds && (ShowNonRenderableBounds || component.IsRenderable))
                        {
                            Tessellator.Quad.DrawRectangleOutline(bounds, new SixLabors.ImageSharp.PixelFormats.Rgba32(0, 0, 255, 255), 1);
                        }

                        if (component.MouseOver && HighlightMouseOver && (HighlightNonRenderable || component.IsRenderable))
                        {
                            Tessellator.Quad.DrawQuad(bounds, new SixLabors.ImageSharp.PixelFormats.Rgba32(255, 0, 255, 50));
                        }

                        if (component.MouseDown && HighlightMouseDown && (HighlightNonRenderable || component.IsRenderable))
                        {
                            Tessellator.Quad.DrawQuad(bounds, new SixLabors.ImageSharp.PixelFormats.Rgba32(255, 0, 0, 50));
                        }
                    }
                }

                if (Canvas.InputCanvas.Target != null)
                {
                    // Draw outline around target bounds
                    if (TargetBoundsOutline)
                    {
                        Tessellator.Quad.DrawRectangleOutline(Canvas.InputCanvas.Target.Bounds.Transform(Canvas.InputCanvas.Target.Transform.Matrix), new SixLabors.ImageSharp.PixelFormats.Rgba32(255, 255, 0, 255), 1);
                    }
                }

                if (Canvas.InputCanvas.LastTarget != null)
                {
                    // Draw outline around target bounds
                    if (LastTargetBoundsOutline)
                    {
                        Tessellator.Quad.DrawRectangleOutline(Canvas.InputCanvas.LastTarget.Bounds.Transform(Canvas.InputCanvas.LastTarget.Transform.Matrix), new SixLabors.ImageSharp.PixelFormats.Rgba32(255, 0, 0, 255), 1);
                    }
                }

                if (FlowLayoutContentBoundsOutline)
                {
                    foreach (var flowLayout in FlowLayout.Instances)
                    {
                        if (flowLayout.Visible)
                        {
                            Tessellator.Quad.DrawRectangleOutline(flowLayout.ContentBounds.Transform(flowLayout.Container.Transform.Matrix), new SixLabors.ImageSharp.PixelFormats.Rgba32(0, 255, 255, 255), 1);
                        }
                    }
                }

                Tessellator.Quad.Flush();
            }
        }
    }
}
