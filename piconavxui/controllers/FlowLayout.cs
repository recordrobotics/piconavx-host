using piconavx.ui.graphics;
using piconavx.ui.graphics.ui;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

namespace piconavx.ui.controllers
{
    public class FlowLayout : Controller
    {
        [Flags]
        public enum AutoSize
        {
            None = 0,
            X = 1 << 0,
            Y = 1 << 1,
            Both = X | Y
        }

        private static List<FlowLayout> instances = new List<FlowLayout>();
        public static IReadOnlyList<FlowLayout> Instances { get { return instances.AsReadOnly(); } }

        public UIController Container { get; }

        public List<UIController> Components { get; }

        public FlowDirection Direction { get; set; }
        public AlignItems AlignItems { get; set; }
        public AlignItems JustifyContent { get; set; }

        public bool Reversed { get; set; } = false;
        public bool Wrap { get; set; } = false;
        public bool Stretch { get; set; } = false;
        public bool Visible { get; set; } = false;

        public AutoSize AutoSizeContainer { get; set; } = AutoSize.Both;
        public float Gap { get; set; } = 0;
        public Insets Padding { get; set; } = new Insets(0);

        private RectangleF contentBounds = default;
        public RectangleF ContentBounds => contentBounds;

        public FlowLayout(UIController container)
        {
            Container = container;
            Components = new List<UIController>();
            Direction = FlowDirection.Vertical;
            AlignItems = AlignItems.Start;
            JustifyContent = AlignItems.Start;
            instances.Add(this);
        }

        ~FlowLayout()
        {
            instances.Remove(this);
        }

        public RectangleF GetAutoSizeBounds()
        {
            float accum = 0;
            float accumAlt = 0;
            RectangleF contentBounds = default;

            for (int i = 0; i < Components.Count; i++)
            {
                var component = Components[Reversed ? (Components.Count - i - 1) : i];
                RectangleF componentBounds = component.Bounds;
                switch (Direction)
                {
                    case FlowDirection.Horizontal:
                        {
                            bool wrap = Wrap && !AutoSizeContainer.HasFlag(AutoSize.X);
                            float x = Container.Bounds.X + Padding.Left + accum;

                            // Overflow
                            if (wrap && contentBounds != default && x + componentBounds.Width > Container.Bounds.Right)
                            {
                                accumAlt = contentBounds.Bottom - Container.Bounds.Top + Gap;
                                accum = 0;
                                x = Container.Bounds.X + Padding.Left + accum;
                            }

                            float y = Container.Bounds.Y + Padding.Top + accumAlt;

                            componentBounds = new RectangleF(x, y, componentBounds.Width, componentBounds.Height);
                            accum += componentBounds.Width;
                            break;
                        }
                    case FlowDirection.Vertical:
                        {
                            bool wrap = Wrap && !AutoSizeContainer.HasFlag(AutoSize.Y);
                            float y = Container.Bounds.Y + Padding.Top + accum;

                            // Overflow
                            if (wrap && contentBounds != default && y + componentBounds.Height > Container.Bounds.Bottom)
                            {
                                accumAlt = contentBounds.Right - Container.Bounds.Left + Gap;
                                accum = 0;
                                y = Container.Bounds.Y + Padding.Top + accum;
                            }

                            float x = Container.Bounds.X + Padding.Left + accumAlt;

                            componentBounds = new RectangleF(x, y, componentBounds.Width, componentBounds.Height);
                            accum += componentBounds.Height;
                            break;
                        }
                }

                if (contentBounds == default)
                    contentBounds = componentBounds;

                if (componentBounds.Left < contentBounds.Left)
                {
                    float right = contentBounds.Right;
                    contentBounds.X = componentBounds.Left;
                    contentBounds.Width += right - contentBounds.Right;
                }

                if (componentBounds.Right > contentBounds.Right)
                {
                    contentBounds.Width += componentBounds.Right - contentBounds.Right;
                }

                if (componentBounds.Top < contentBounds.Top)
                {
                    float bottom = contentBounds.Bottom;
                    contentBounds.Y = componentBounds.Top;
                    contentBounds.Height += bottom - contentBounds.Bottom;
                }

                if (componentBounds.Bottom > contentBounds.Bottom)
                {
                    var dif = componentBounds.Bottom - contentBounds.Bottom;
                    contentBounds.Height += dif;
                }

                accum += Gap;
            }

            return new RectangleF(Container.Bounds.X, Container.Bounds.Y, AutoSizeContainer.HasFlag(AutoSize.X) ? (contentBounds.Width + Padding.Horizontal) : Container.Bounds.Width, AutoSizeContainer.HasFlag(AutoSize.Y) ? (contentBounds.Height + Padding.Vertical) : Container.Bounds.Height);
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
            if (AutoSizeContainer != AutoSize.None)
                Container.Bounds = GetAutoSizeBounds();

            float accum = 0;
            float accumAlt = 0;
            RectangleF contentBounds = default;

            for (int i = 0; i < Components.Count; i++)
            {
                var component = Components[Reversed ? (Components.Count - i - 1) : i];

                switch (Direction)
                {
                    case FlowDirection.Horizontal:
                        {
                            float x = Container.Bounds.X + Padding.Left + accum;

                            // Overflow
                            if (Wrap && contentBounds != default && x + component.Bounds.Width > Container.Bounds.Right)
                            {
                                accumAlt = contentBounds.Bottom - Container.Bounds.Top + Gap;
                                accum = 0;
                                x = Container.Bounds.X + Padding.Left + accum;
                            }

                            float y = Container.Bounds.Y + Padding.Top + accumAlt;

                            switch (AlignItems)
                            {
                                case AlignItems.Middle:
                                    y += (Container.Bounds.Height - Padding.Vertical) / 2 - component.Bounds.Height / 2;
                                    break;
                                case AlignItems.End:
                                    y = Container.Bounds.Bottom - Padding.Bottom - component.Bounds.Height;
                                    break;
                            }

                            component.Bounds = new RectangleF(x, y, component.Bounds.Width, component.Bounds.Height);
                            accum += component.Bounds.Width;
                            break;
                        }
                    case FlowDirection.Vertical:
                        {
                            float y = Container.Bounds.Y + Padding.Top + accum;

                            // Overflow
                            if (Wrap && contentBounds != default && y + component.Bounds.Height > Container.Bounds.Bottom)
                            {
                                accumAlt = contentBounds.Right - Container.Bounds.Left + Gap;
                                accum = 0;
                                y = Container.Bounds.Y + Padding.Top + accum;
                            }

                            float x = Container.Bounds.X + Padding.Left + accumAlt;

                            switch (AlignItems)
                            {
                                case AlignItems.Middle:
                                    x += (Container.Bounds.Width - Padding.Horizontal) / 2 - component.Bounds.Width / 2;
                                    break;
                                case AlignItems.End:
                                    x = Container.Bounds.Right - Padding.Right - component.Bounds.Width;
                                    break;
                            }

                            component.Bounds = new RectangleF(x, y, component.Bounds.Width, component.Bounds.Height);
                            accum += component.Bounds.Height;
                            break;
                        }
                }

                if (contentBounds == default)
                    contentBounds = component.Bounds;

                if (component.Bounds.Left < contentBounds.Left)
                {
                    float right = contentBounds.Right;
                    contentBounds.X = component.Bounds.Left;
                    contentBounds.Width += right - contentBounds.Right;
                }

                if (component.Bounds.Right > contentBounds.Right)
                {
                    contentBounds.Width += component.Bounds.Right - contentBounds.Right;
                }

                if (component.Bounds.Top < contentBounds.Top)
                {
                    float bottom = contentBounds.Bottom;
                    contentBounds.Y = component.Bounds.Top;
                    contentBounds.Height += bottom - contentBounds.Bottom;
                }

                if (component.Bounds.Bottom > contentBounds.Bottom)
                {
                    var dif = component.Bounds.Bottom - contentBounds.Bottom;
                    contentBounds.Height += dif;
                }

                if (!Stretch)
                {
                    accum += Gap;
                }
                else if (i < Components.Count - 1)
                {
                    var nextComponent = Components[Reversed ? (Components.Count - i) : (i + 1)];
                    switch (Direction)
                    {
                        case FlowDirection.Horizontal:
                            accum += Container.Bounds.Width - component.Bounds.Width - nextComponent.Bounds.Width;
                            break;
                        case FlowDirection.Vertical:
                            accum += Container.Bounds.Height - component.Bounds.Height - nextComponent.Bounds.Height;
                            break;
                    }
                }
            }

            switch (JustifyContent)
            {
                case AlignItems.Middle:
                    {
                        switch (Direction)
                        {
                            case FlowDirection.Horizontal:
                                {
                                    RectangleF newContentBounds = new RectangleF(Container.Bounds.Left + Container.Bounds.Width / 2 - contentBounds.Width / 2, contentBounds.Y, contentBounds.Width, contentBounds.Height);
                                    foreach (var component in Components)
                                    {
                                        component.Bounds = new(component.Bounds.Left - contentBounds.Left + newContentBounds.Left, component.Bounds.Y, component.Bounds.Width, component.Bounds.Height);
                                    }
                                    contentBounds = newContentBounds;
                                }
                                break;
                            case FlowDirection.Vertical:
                                {
                                    RectangleF newContentBounds = new RectangleF(contentBounds.X, Container.Bounds.Top + Container.Bounds.Height / 2 - contentBounds.Height / 2, contentBounds.Width, contentBounds.Height);
                                    foreach (var component in Components)
                                    {
                                        component.Bounds = new(component.Bounds.X, component.Bounds.Top - contentBounds.Top + newContentBounds.Top, component.Bounds.Width, component.Bounds.Height);
                                    }
                                    contentBounds = newContentBounds;
                                }
                                break;
                        }
                    }
                    break;
                case AlignItems.End:
                    {
                        switch (Direction)
                        {
                            case FlowDirection.Horizontal:
                                {
                                    RectangleF newContentBounds = new RectangleF(Container.Bounds.Right - contentBounds.Width, contentBounds.Y, contentBounds.Width, contentBounds.Height);
                                    foreach (var component in Components)
                                    {
                                        component.Bounds = new(component.Bounds.Left - contentBounds.Left + newContentBounds.Left, component.Bounds.Y, component.Bounds.Width, component.Bounds.Height);
                                    }
                                    contentBounds = newContentBounds;
                                }
                                break;
                            case FlowDirection.Vertical:
                                {
                                    RectangleF newContentBounds = new RectangleF(contentBounds.X, Container.Bounds.Bottom - contentBounds.Height, contentBounds.Width, contentBounds.Height);
                                    foreach (var component in Components)
                                    {
                                        component.Bounds = new(component.Bounds.X, component.Bounds.Top - contentBounds.Top + newContentBounds.Top, component.Bounds.Width, component.Bounds.Height);
                                    }
                                    contentBounds = newContentBounds;
                                }
                                break;
                        }
                    }
                    break;
            }

            this.contentBounds = contentBounds;
        }
    }

    public enum FlowDirection
    {
        Horizontal,
        Vertical
    }

    public enum AlignItems
    {
        Start,
        Middle,
        End
    }
}
