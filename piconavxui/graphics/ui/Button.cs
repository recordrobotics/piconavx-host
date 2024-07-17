using FontStashSharp;
using piconavx.ui.controllers;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace piconavx.ui.graphics.ui
{
    public class Button : UIController
    {
        public enum AutoSizeMode
        {
            None,
            TextOrIconOnly,
            TextAndIcon
        }

        public interface ButtonColor
        {
            public Rgba32 Background { get; }
            public Rgba32 BackgroundDisabled { get; }
            public Rgba32 BackgroundHover { get; }
            public Rgba32 BackgroundActive { get; }

            public Rgba32 Color { get; }
            public Rgba32 ColorDisabled { get; }

            public static readonly ButtonColor Neutral = new Neutral();
            public static readonly ButtonColor Primary = new Primary();
            public static readonly ButtonColor Success = new Success();
            public static readonly ButtonColor Error = new Neutral();
        }

        public struct Neutral : ButtonColor
        {
            public readonly Rgba32 Background => Rgba32.ParseHex("#383838");
            public readonly Rgba32 BackgroundDisabled => Rgba32.ParseHex("#242424");
            public readonly Rgba32 BackgroundHover => Rgba32.ParseHex("#424242");
            public readonly Rgba32 BackgroundActive => Rgba32.ParseHex("#4d4d4d");
            public readonly Rgba32 Color => Rgba32.ParseHex("#fff");
            public readonly Rgba32 ColorDisabled => Rgba32.ParseHex("#b3b3b3");
        }

        public struct Primary : ButtonColor
        {
            public readonly Rgba32 Background => Rgba32.ParseHex("#2e65c9");
            public readonly Rgba32 BackgroundDisabled => Rgba32.ParseHex("#0e2247");
            public readonly Rgba32 BackgroundHover => Rgba32.ParseHex("#3b76e3");
            public readonly Rgba32 BackgroundActive => Rgba32.ParseHex("#2a5dbd");
            public readonly Rgba32 Color => Rgba32.ParseHex("#fff");
            public readonly Rgba32 ColorDisabled => Rgba32.ParseHex("#b3b3b3");
        }

        public struct Success : ButtonColor
        {
            public readonly Rgba32 Background => Rgba32.ParseHex("#0d1a0e");
            public readonly Rgba32 BackgroundDisabled => Rgba32.ParseHex("#151c16");
            public readonly Rgba32 BackgroundHover => Rgba32.ParseHex("#152b17");
            public readonly Rgba32 BackgroundActive => Rgba32.ParseHex("#1b381d");
            public readonly Rgba32 Color => Rgba32.ParseHex("#c3f7be");
            public readonly Rgba32 ColorDisabled => Rgba32.ParseHex("#72916e");
        }

        public Button(string text, Canvas canvas) : base(canvas)
        {
            background = new Image(canvas);
            background.ZIndex = ZIndex; // background
            background.HitTestAlphaClip = 0.9f;
            background.Color = Color.Background;
            background.Texture = Texture.RoundedRect;
            background.ImageType = ImageType.Sliced;
            background.Size = new Size(15, 15);
            background.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, () =>
            {
                if (!Disabled) // Disabled handling
                    NotifyClick();
            });
            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            RaycastTransparency = RaycastTransparency.Transparent;

            icon = new Image(canvas);
            icon.ZIndex = ContentZIndex;
            icon.RaycastTransparency = RaycastTransparency.Hidden;
            icon.Color = Color.Color;
            icon.Bounds = new RectangleF(0, 0, 0, 20);
            iconAnchor = new AnchorLayout(icon, this);
            iconAnchor.Anchor = isIconButton ? Anchor.All : (Anchor.TopLeft | Anchor.Bottom);
            iconAnchor.Insets = new Insets(padding.Left, padding.Top, padding.Right, padding.Top); // padding.Top for bottom to keep symmetry
            iconAnchor.AllowResize = false;

            this.text = new Label(text, canvas);
            this.text.FontSize = 10;
            this.text.ZIndex = ContentZIndex;
            this.text.Color = new FSColor(Color.Color.ToVector4());
            textAnchor = new AnchorLayout(this.text, this);
            textAnchor.Anchor = Anchor.TopLeft | Anchor.Bottom;
            textAnchor.Insets = new Insets(padding.Left + (Icon == null ? 0 : iconSize.Width + iconGap), padding.Top, padding.Right, padding.Bottom);
            textAnchor.AllowResize = false; // instead of stretching, center it

            bounds = GetAutoSizeBounds();
        }

        private Image background;
        private Image icon;
        private Label text;
        private AnchorLayout backgroundAnchor;
        private AnchorLayout iconAnchor;
        private AnchorLayout textAnchor;

        public ButtonColor Color { get; set; } = ButtonColor.Neutral;

        public string Text { get => this.text.Text; set => this.text.Text = value; }
        public float FontSize { get => this.text.FontSize; set => this.text.FontSize = value; }
        public Vector2 RenderOffset { get => this.text.RenderOffset; set => this.text.RenderOffset = value; }

        private AutoSizeMode autoSize = AutoSizeMode.TextOrIconOnly;
        public AutoSizeMode AutoSize { get => autoSize; set => autoSize = value; }

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

        private SizeF iconSize = new SizeF(30, 30);
        public SizeF IconSize
        {
            get => iconSize; set
            {
                iconSize = value;
                icon.Bounds = (Icon == null && !isIconButton) ? new RectangleF(0, 0, 0, iconSize.Height) : new RectangleF(0, 0, iconSize.Width, iconSize.Height);
                textAnchor.Insets = new Insets(padding.Left + (Icon == null ? 0 : iconSize.Width + iconGap), padding.Top, padding.Right, padding.Bottom);
            }
        }

        private Insets padding = new Insets(20, 16, 20, 13);
        public Insets Padding
        {
            get => padding; set
            {
                padding = value;
                iconAnchor.Insets = new Insets(padding.Left, padding.Top, padding.Right, padding.Top); // padding.Top for bottom to keep symmetry
                textAnchor.Insets = new Insets(padding.Left + (Icon == null ? 0 : iconSize.Width + iconGap), padding.Top, padding.Right, padding.Bottom);
            }
        }

        private float iconGap = 13;
        public float IconGap
        {
            get => iconGap; set
            {
                iconGap = value;
                textAnchor.Insets = new Insets(padding.Left + (Icon == null ? 0 : iconSize.Width + iconGap), padding.Top, padding.Right, padding.Bottom);
            }
        }

        public override RaycastTransparency RaycastTransparency { get => base.RaycastTransparency; set => base.RaycastTransparency = background.RaycastTransparency = value; }
        public override bool IsRenderable => false;
        public override bool MouseDown { get => background.MouseDown; set => background.MouseDown = value; }
        public override bool MouseOver { get => background.MouseOver; set => background.MouseOver = value; }

        public Texture? Icon
        {
            get => icon.Texture; set
            {
                icon.Texture = value;
                icon.Bounds = (value == null && !isIconButton) ? new RectangleF(0, 0, 0, iconSize.Height) : new RectangleF(0, 0, iconSize.Width, iconSize.Height);
                textAnchor.Insets = new Insets(padding.Left + (value == null ? 0 : iconSize.Width + iconGap), padding.Top, padding.Right, padding.Bottom);
            }
        }

        private bool isIconButton = false;
        public bool IsIconButton
        {
            get => isIconButton; set
            {
                isIconButton = value;
                iconAnchor.Anchor = isIconButton ? Anchor.All : (Anchor.TopLeft | Anchor.Bottom);
                icon.Bounds = (Icon == null && !isIconButton) ? new RectangleF(0, 0, 0, iconSize.Height) : new RectangleF(0, 0, iconSize.Width, iconSize.Height);
                if (isIconButton)
                {
                    Canvas.RemoveComponent(text);
                }
                else
                {
                    Canvas.AddComponent(text);
                }

                if (autoSize != AutoSizeMode.None)
                {
                    bounds = GetAutoSizeBounds();
                }
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
                switch (autoSize)
                {
                    case AutoSizeMode.TextOrIconOnly:
                        return new RectangleF(bounds.X, bounds.Y, textSize.Width + padding.Horizontal + (Icon == null ? 0 : iconSize.Width + iconGap), textSize.Height + padding.Vertical);
                    case AutoSizeMode.TextAndIcon:
                        return new RectangleF(bounds.X, bounds.Y, textSize.Width + padding.Horizontal + (Icon == null ? 0 : iconSize.Width + iconGap), Math.Max(iconSize.Height, textSize.Height) + padding.Vertical);
                }
            }

            return bounds;
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

        public override void OnAdd()
        {
            base.OnAdd();
            Canvas.AddComponent(background);
            Canvas.AddComponent(icon);
            if (!IsIconButton)
                Canvas.AddComponent(text);
        }

        public override void OnRemove()
        {
            base.OnRemove();
            Canvas.RemoveComponent(background);
            Canvas.RemoveComponent(icon);
            Canvas.RemoveComponent(text);
        }

        private void Scene_Update(double deltaTime)
        {
            if (autoSize != AutoSizeMode.None)
            {
                bounds = GetAutoSizeBounds();
            }

            background.Color = isDisabled ? Color.BackgroundDisabled : MouseDown ? Color.BackgroundActive : MouseOver ? Color.BackgroundHover : Color.Background;
            icon.Color = isDisabled ? Color.ColorDisabled : Color.Color;
            text.Color = new FSColor((isDisabled ? Color.ColorDisabled : Color.Color).ToVector4());
        }
    }
}
