using piconavx.ui.controllers;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Numerics;

namespace piconavx.ui.graphics.ui
{
    public class ScrollPanel : Panel
    {
        public readonly Rgba32 SCROLL_THUMB = Rgba32.ParseHex("#383838");
        public readonly Rgba32 SCROLL_THUMB_HOVER = Rgba32.ParseHex("#424242");
        public readonly Rgba32 SCROLL_THUMB_ACTIVE = Rgba32.ParseHex("#4d4d4d");

        const float scrollWidth = 20;

        public bool Horizontal { get; set; } = false;
        public bool Vertical { get; set; } = true;

        public UIController Content { get; set; }

        public Insets Padding { get; set; } = new Insets(0);

        private RectangleF workingRectangle = default;
        private VirtualUIController virtualWorkingRectangle;

        public RectangleF WorkingRectangle => workingRectangle;
        public VirtualUIController VirtualWorkingRectangle => virtualWorkingRectangle;

        private bool horVisible = false;
        private bool verVisible = false;

        public bool HorizontalVisible => horVisible;
        public bool VerticalVisible => verVisible;

        private float offsetX = 0;
        private float offsetY = 0;
        public float OffsetX => offsetX;
        public float OffsetY => offsetY;

        private Image scrollHor;
        private Image scrollVer;
        private AnchorLayout scrollHorAnchor;
        private AnchorLayout scrollVerAnchor;
        private bool prevHorDown;
        private bool prevVerDown;
        private float horStart;
        private float verStart;
        private float mouseStart;

        private float scrollY = 0;

        public override bool MouseDown { get => base.MouseDown || scrollVer.MouseDown; set => base.MouseDown = value; }
        public override bool MouseOver { get => base.MouseOver || scrollVer.MouseOver; set => base.MouseOver = value; }

        public ScrollPanel(Canvas canvas, UIController content) : base(canvas)
        {
            SupportsInputEvents = true;
            RaycastTransparency = RaycastTransparency.Opaque;
            SecondaryInputVisible = true;

            Content = content;
            virtualWorkingRectangle = new VirtualUIController(canvas);
            virtualWorkingRectangle.GetBounds = () => workingRectangle;

            scrollHor = new Image(canvas);
            scrollHor.HitTestAlphaClip = 0.9f;
            scrollHor.RaycastTransparency = RaycastTransparency.Transparent;
            scrollHor.Color = SCROLL_THUMB;
            scrollHor.Texture = Texture.Pill;
            scrollHor.ImageType = ImageType.Sliced;
            scrollHor.Size = new Size((int)scrollWidth / 2, (int)scrollWidth / 2);
            scrollHor.Bounds = new RectangleF(0, 0, 400, scrollWidth);
            scrollHorAnchor = new AnchorLayout(scrollHor, this);
            scrollHorAnchor.Anchor = Anchor.Bottom;
            scrollHorAnchor.Insets = new Insets(0);

            scrollVer = new Image(canvas);
            scrollVer.HitTestAlphaClip = 0.9f;
            scrollVer.RaycastTransparency = RaycastTransparency.Transparent;
            scrollVer.Color = SCROLL_THUMB;
            scrollVer.Texture = Texture.Pill;
            scrollVer.ImageType = ImageType.Sliced;
            scrollVer.Size = new Size((int)scrollWidth / 2, (int)scrollWidth / 2);
            scrollVer.Bounds = new RectangleF(0, 0, scrollWidth, 400);
            scrollVerAnchor = new AnchorLayout(scrollVer, this);
            scrollVerAnchor.Anchor = Anchor.Right;
            scrollVerAnchor.Insets = new Insets(0);

            UpdateZIndex();
        }

        protected override void UpdateZIndex()
        {
            scrollHor.ZIndex = ZIndex + 1;
            scrollVer.ZIndex = ZIndex + 1;
        }

        public void CalculateWorkingRectangle()
        {
            workingRectangle = new RectangleF(Bounds.X + Padding.Left, Bounds.Y + Padding.Top, Bounds.Width - Padding.Horizontal - (verVisible ? scrollWidth : 0), Bounds.Height - Padding.Vertical - (horVisible ? scrollWidth : 0));
        }

        public override void Subscribe()
        {
            scrollHor.Subscribe();
            scrollHorAnchor.Subscribe();
            scrollVer.Subscribe();
            scrollVerAnchor.Subscribe();

            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
            Scroll += new PrioritizedAction<GenericPriority, Vector2>(GenericPriority.Highest, MouseScroll);
        }

        public override void Unsubscribe()
        {
            scrollHor.Unsubscribe();
            scrollHorAnchor.Unsubscribe();
            scrollVer.Unsubscribe();
            scrollVerAnchor.Unsubscribe();

            Scene.Update -= Scene_Update;
            Scroll -= MouseScroll;
        }

        public override void OnAdd()
        {
            base.OnAdd();
            if (horVisible)
                Canvas.AddComponent(scrollHor);
            if (verVisible)
                Canvas.AddComponent(scrollVer);
        }

        public override void OnRemove()
        {
            if (horVisible)
                Canvas.RemoveComponent(scrollHor);
            if (verVisible)
                Canvas.RemoveComponent(scrollVer);
        }

        private void MouseScroll(Vector2 scrollWheel)
        {
            scrollY += scrollWheel.Y;
        }

        private void Scene_Update(double deltaTime)
        {
            CalculateWorkingRectangle();
            bool prevHorVisible = horVisible;
            bool prevVerVisible = verVisible;
            horVisible = Horizontal && Content.Bounds.Width > workingRectangle.Width;
            verVisible = Vertical && Content.Bounds.Height > workingRectangle.Height;
            CalculateWorkingRectangle();

            if (prevHorVisible && !horVisible)
            {
                Canvas.RemoveComponent(scrollHor);
            }
            else if (!prevHorVisible && horVisible)
            {
                Canvas.AddComponent(scrollHor);
            }

            if (prevVerVisible && !verVisible)
            {
                Canvas.RemoveComponent(scrollVer);
            }
            else if (!prevVerVisible && verVisible)
            {
                Canvas.AddComponent(scrollVer);
            }

            if (horVisible)
            {
                float mouse = (Window.Current.Input?.Mice.FirstOrDefault()?.Position.X) ?? 0;

                if (!prevHorDown && scrollHor.MouseDown)
                {
                    mouseStart = mouse;
                    horStart = scrollHor.Bounds.X - workingRectangle.X;
                }

                if (scrollHor.MouseDown)
                {
                    float dm = mouse - mouseStart;
                    offsetX = (horStart + dm) / workingRectangle.Width * Content.Bounds.Width;
                }

                offsetX = MathF.Max(0, MathF.Min(offsetX, MathF.Max(0, Content.Bounds.Width - workingRectangle.Width)));

                scrollHor.Bounds = new RectangleF(workingRectangle.X + (offsetX / Content.Bounds.Width * workingRectangle.Width), scrollHor.Bounds.Y, MathF.Max(workingRectangle.Width / Content.Bounds.Width * workingRectangle.Width, scrollWidth * 2), scrollHor.Bounds.Height);
                scrollHor.Color = scrollHor.MouseDown ? SCROLL_THUMB_ACTIVE : scrollHor.MouseOver ? SCROLL_THUMB_HOVER : SCROLL_THUMB;

                prevHorDown = scrollHor.MouseDown;
            }

            if (verVisible)
            {
                float mouse = (Window.Current.Input?.Mice.FirstOrDefault()?.Position.Y) ?? 0;

                if (!prevVerDown && scrollVer.MouseDown)
                {
                    mouseStart = mouse;
                    verStart = scrollVer.Bounds.Y - workingRectangle.Y;
                }

                if (scrollVer.MouseDown)
                {
                    float dm = mouse - mouseStart;
                    offsetY = (verStart + dm) / workingRectangle.Height * Content.Bounds.Height;
                }

                if (scrollY != 0)
                {
                    offsetY -= scrollY * 50;
                    Scene.InvokeLater(Canvas.InvalidateInput, DeferralMode.NextFrame, 2);
                }

                offsetY = MathF.Max(0, MathF.Min(offsetY, MathF.Max(0, Content.Bounds.Height - workingRectangle.Height)));

                scrollVer.Bounds = new RectangleF(scrollVer.Bounds.X, workingRectangle.Y + (offsetY / Content.Bounds.Height * workingRectangle.Height), scrollVer.Bounds.Width, MathF.Max(workingRectangle.Height / Content.Bounds.Height * workingRectangle.Height, scrollWidth * 2));
                scrollVer.Color = scrollVer.MouseDown ? SCROLL_THUMB_ACTIVE : scrollVer.MouseOver ? SCROLL_THUMB_HOVER : SCROLL_THUMB;

                prevVerDown = scrollVer.MouseDown;
            }
            else
            {
                prevVerDown = false;
            }

            scrollY = 0;

            Content.Bounds = new RectangleF(workingRectangle.X - offsetX, workingRectangle.Y - offsetY, Content.Bounds.Width, Content.Bounds.Height);
        }
    }
}
