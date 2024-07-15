using FontStashSharp;
using piconavx.ui.controllers;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace piconavx.ui.graphics.ui
{
    public class Button : UIController
    {
        public readonly Rgba32 BACKGROUND = Rgba32.ParseHex("#383838");
        public readonly Rgba32 BACKGROUND_DISABLED = Rgba32.ParseHex("#242424");
        public readonly Rgba32 BACKGROUND_HOVER = Rgba32.ParseHex("#424242");
        public readonly Rgba32 BACKGROUND_ACTIVE = Rgba32.ParseHex("#4d4d4d");
        public readonly Rgba32 COLOR = Rgba32.ParseHex("#fff");
        public readonly Rgba32 COLOR_DISABLED = Rgba32.ParseHex("#b3b3b3");

        public Button(string text, Canvas canvas) : base(canvas)
        {
            background = new Image(canvas);
            canvas.AddComponent(background);
            background.ZIndex = ZIndex; // background
            background.HitTestAlphaClip = 0.9f;
            background.Color = BACKGROUND;
            background.Texture = Texture.RoundedRect;
            background.ImageType = ImageType.Sliced;
            background.Size = new Size(15, 15);
            background.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, NotifyClick);
            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            RaycastTransparency = RaycastTransparency.Transparent;

            icon = new Image(canvas);
            canvas.AddComponent(icon);
            icon.ZIndex = ContentZIndex;
            icon.RaycastTransparency = RaycastTransparency.Hidden;
            icon.Color = COLOR;
            icon.Texture = Texture.UVTest;
            icon.Bounds = new RectangleF(0, 0, 20, 20);
            iconAnchor = new AnchorLayout(icon, this);
            iconAnchor.Anchor = isIconButton ? Anchor.All : (Anchor.TopLeft | Anchor.Bottom);
            iconAnchor.Insets = new Insets(padding.Left, padding.Top, padding.Right, padding.Top); // padding.Top for bottom to keep symmetry
            iconAnchor.AllowResize = false;

            this.text = new Label(text, canvas);
            canvas.AddComponent(this.text);
            this.text.FontSize = 10;
            this.text.ZIndex = ContentZIndex;
            this.text.Color = new FSColor(COLOR.ToVector4());
            textAnchor = new AnchorLayout(this.text, this);
            textAnchor.Anchor = Anchor.TopLeft | Anchor.Bottom;
            textAnchor.Insets = new Insets(padding.Left + 33, padding.Top, padding.Right, padding.Bottom);
            textAnchor.AllowResize = false; // instead of stretching, center it

            bounds = GetAutoSizeBounds();
        }

        private Image background;
        private Image icon;
        private Label text;
        private AnchorLayout backgroundAnchor;
        private AnchorLayout iconAnchor;
        private AnchorLayout textAnchor;

        public string Text { get => this.text.Text; set => this.text.Text = value; }

        private bool autoSize = true;
        public bool AutoSize { get => autoSize; set => autoSize = value; }

        private int zIndex = 0;
        public override int ZIndex
        {
            get => zIndex; set
            {
                zIndex = value;
                background.ZIndex = zIndex;
                icon.ZIndex = ContentZIndex;
                text.ZIndex = ContentZIndex;
                Canvas.InvalidateHierarchy();
            }
        }

        public int ContentZIndex => zIndex + 1;

        private RectangleF bounds;
        public override RectangleF Bounds { get => bounds; set => bounds = value; }

        private Insets padding = new Insets(20, 16, 20, 13);
        public Insets Padding { get => padding; set => padding = value; }

        public override RaycastTransparency RaycastTransparency { get => base.RaycastTransparency; set => base.RaycastTransparency = background.RaycastTransparency = value; }
        public override bool IsRenderable => false;
        public override bool MouseDown { get => background.MouseDown; set => background.MouseDown = value; }
        public override bool MouseOver { get => background.MouseOver; set => background.MouseOver = value; }

        public Texture? Icon { get => icon.Texture; set => icon.Texture = value; }

        private bool isIconButton = false;
        public bool IsIconButton
        {
            get => isIconButton; set
            {
                isIconButton = value;
                iconAnchor.Anchor = isIconButton ? Anchor.All : (Anchor.TopLeft | Anchor.Bottom);
                if (isIconButton)
                {
                    Canvas.RemoveComponent(text);
                }
                else
                {
                    Canvas.AddComponent(text);
                }
                bounds = GetAutoSizeBounds();
            }
        }

        private bool isDisabled = false;
        public bool Disabled { get => isDisabled; set => isDisabled = value; }

        public RectangleF GetAutoSizeBounds()
        {
            if (isIconButton)
            {
                return new RectangleF(bounds.X, bounds.Y, icon.Bounds.Width + padding.Horizontal, icon.Bounds.Height + padding.Vertical);
            }
            else
            {
                var textSize = text.GetAutoSizeBounds();
                return new RectangleF(bounds.X, bounds.Y, textSize.Width + padding.Horizontal + 33, textSize.Height + padding.Vertical);
            }
        }

        public override void Subscribe()
        {
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
            background.Subscribe();
            icon.Subscribe();
            text.Subscribe();
            backgroundAnchor.Subscribe();
            iconAnchor.Subscribe();
            textAnchor.Subscribe();
        }

        public override void Unsubscribe()
        {
            Scene.Update -= Scene_Update;
            background.Unsubscribe();
            icon.Unsubscribe();
            text.Unsubscribe();
            backgroundAnchor.Unsubscribe();
            iconAnchor.Unsubscribe();
            textAnchor.Unsubscribe();
        }

        private void Scene_Update(double deltaTime)
        {
            if (autoSize)
            {
                bounds = GetAutoSizeBounds();
            }

            background.Color = isDisabled ? BACKGROUND_DISABLED : MouseDown ? BACKGROUND_ACTIVE : MouseOver ? BACKGROUND_HOVER : BACKGROUND;
            icon.Color = isDisabled ? COLOR_DISABLED : COLOR;
            text.Color = new FSColor((isDisabled ? COLOR_DISABLED : COLOR).ToVector4());
        }
    }
}
