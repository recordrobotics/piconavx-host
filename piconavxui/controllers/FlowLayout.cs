﻿using piconavx.ui.graphics;
using piconavx.ui.graphics.ui;
using System.Drawing;

namespace piconavx.ui.controllers
{
    public class FlowLayout : Controller
    {
        public UIController Container { get; }

        public List<UIController> Components { get; }

        public FlowDirection Direction { get; set; }

        public bool Reversed { get; set; } = false;

        public bool AutoSizeContainer { get; set; } = true;
        public float Gap { get; set; } = 0;

        public FlowLayout(UIController container)
        {
            Container = container;
            Components = new List<UIController>();
            Direction = FlowDirection.Vertical;
        }

        public RectangleF GetAutoSizeBounds()
        {
            switch (Direction)
            {
                case FlowDirection.Horizontal:
                    {
                        float width = 0;
                        float height = 0;

                        foreach (var component in Components)
                        {
                            width += component.Bounds.Width;
                            height = MathF.Max(height, component.Bounds.Height);
                        }

                        return new RectangleF(Container.Bounds.X, Container.Bounds.Y, width + Gap * (Components.Count - 1), height);
                    }
                case FlowDirection.Vertical:
                    {
                        float width = 0;
                        float height = 0;

                        foreach (var component in Components)
                        {
                            width = MathF.Max(width, component.Bounds.Width);
                            height += component.Bounds.Height;
                        }

                        return new RectangleF(Container.Bounds.X, Container.Bounds.Y, width, height + Gap * (Components.Count - 1));
                    }
            }

            return Container.Bounds;
        }

        public override void Subscribe()
        {
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
        }

        public override void Unsubscribe()
        {
            Scene.Update -= Scene_Update;
        }

        private void Scene_Update(double deltaTime)
        {
            if (AutoSizeContainer)
                Container.Bounds = GetAutoSizeBounds();

            float accum = 0;
            for (int i = 0; i < Components.Count; i++)
            {
                var component = Components[Reversed ? (Components.Count - i - 1) : i];

                switch (Direction)
                {
                    case FlowDirection.Horizontal:
                        {
                            component.Bounds = new RectangleF(Container.Bounds.X + accum, Container.Bounds.Y, component.Bounds.Width, component.Bounds.Height);
                            accum += component.Bounds.Width;
                            break;
                        }
                    case FlowDirection.Vertical:
                        {
                            component.Bounds = new RectangleF(Container.Bounds.X, Container.Bounds.Y + accum, component.Bounds.Width, component.Bounds.Height);
                            accum += component.Bounds.Height;
                            break;
                        }
                }

                accum += Gap;
            }
        }
    }

    public enum FlowDirection
    {
        Horizontal,
        Vertical
    }
}
