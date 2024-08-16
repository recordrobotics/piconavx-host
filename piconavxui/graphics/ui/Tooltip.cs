using FontStashSharp;
using piconavx.ui.controllers;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;
using System.Numerics;
using static piconavx.ui.graphics.ui.Button;

namespace piconavx.ui.graphics.ui
{
    public class Tooltip : UIController
    {
        private static Texture? cardShadowTexture;

        public Tooltip(string text, string description, UIController target, Canvas canvas) : base(canvas)
        {
            cardShadowTexture ??= Scene.AddResource(new Texture("assets/textures/cardshadow.png")
            {
                Border = new Insets(32),
                WrapMode = TextureWrapMode.Clamp
            });

            popupLayout = new PopupLayout(this, target);
            popupLayout.UseTransform = true;
            popupLayout.Anchor = PopupAnchor.Left;
            popupLayout.Offset = new Vector2(10, 0);

            background = new Image(canvas);
            background.ZIndex = ZIndex + 1; // background
            background.HitTestAlphaClip = 0.9f;
            background.Color = Color.Background;
            background.Texture = Texture.RoundedRect;
            background.ImageType = ImageType.Sliced;
            background.Size = new Size(10, 10);
            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = controllers.Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            shadow = new Image(canvas);
            shadow.RaycastTransparency = RaycastTransparency.Hidden;
            shadow.ZIndex = ZIndex;
            shadow.Color = Theme.TooltipShadow;
            shadow.Texture = cardShadowTexture;
            shadow.ImageType = ImageType.Sliced;
            shadow.Size = new Size(25, 25);
            shadowAnchor = new AnchorLayout(shadow, this);
            shadowAnchor.Anchor = controllers.Anchor.All;
            shadowAnchor.Insets = new Insets(-shadow.Size.Width + background.Size.Width, 8 - shadow.Size.Height + background.Size.Height, -shadow.Size.Width + background.Size.Width, -8 - shadow.Size.Height + background.Size.Height);

            RaycastTransparency = RaycastTransparency.Hidden;

            this.text = new Label(text, canvas);
            this.text.FontSize = 14;
            this.text.ZIndex = ContentZIndex;
            this.text.Color = Color.Text;

            this.description = new Label(description, canvas);
            this.description.FontSize = 12;
            this.description.ZIndex = ContentZIndex;
            this.description.Color = Color.TextSecondary;

            flow = new FlowLayout(this);
            flow.Direction = FlowDirection.Vertical;
            flow.Padding = padding;
            flow.Components.Add(this.text);
            flow.Components.Add(this.description);

            bounds = GetAutoSizeBounds();
        }

        private Image background;
        private FlowLayout flow;
        private Label text;
        private Label description;
        private AnchorLayout backgroundAnchor;
        private Image shadow;
        private AnchorLayout shadowAnchor;

        private PopupLayout popupLayout;

        public ButtonColor Color { get; set; } = Theme.Neutral;

        public string Text { get => this.text.Text; set => this.text.Text = value; }
        public float FontSize { get => this.text.FontSize; set => this.text.FontSize = value; }
        public string Description { get => this.description.Text; set => this.description.Text = value; }
        public float DescriptionFontSize { get => this.description.FontSize; set => this.description.FontSize = value; }

        public PopupAnchor Anchor { get => this.popupLayout.Anchor; set => this.popupLayout.Anchor = value; }
        public Vector2 Offset { get => this.popupLayout.Offset; set => this.popupLayout.Offset = value; }

        private bool autoSize = true;
        public bool AutoSize { get => autoSize; set => autoSize = value; }

        private int zIndex = 80;
        public override int ZIndex
        {
            get => zIndex; set
            {
                zIndex = value;
                background.ZIndex = zIndex + 1;
                shadow.ZIndex = zIndex;
                text.ZIndex = ContentZIndex;
                description.ZIndex = ContentZIndex;
                Canvas.InvalidateHierarchy();
            }
        }

        public int ContentZIndex => zIndex + 2;

        private RectangleF bounds;
        public override RectangleF Bounds
        {
            get => bounds; set
            {
                bounds = value;
            }
        }


        private Insets padding = new Insets(15, 6.5f, 15, 6.5f);
        public Insets Padding
        {
            get => padding; set
            {
                padding = value;
                flow.Padding = new Insets(padding.Left, padding.Top, padding.Right, padding.Bottom);
            }
        }

        public override RaycastTransparency RaycastTransparency { get => base.RaycastTransparency; set => base.RaycastTransparency = background.RaycastTransparency = value; }
        public override bool IsRenderable => false;
        public override bool MouseDown { get => background.MouseDown; set => background.MouseDown = value; }
        public override bool MouseOver { get => background.MouseOver; set => background.MouseOver = value; }

        private bool shown = false;
        public bool Shown => shown;

        private double showDelay = 0.2;
        public double ShowDelay { get => showDelay; set => showDelay = value; }

        private double hideDelay = 0.1;
        public double HideDelay { get => hideDelay; set => hideDelay = value; }

        public RectangleF GetAutoSizeBounds()
        {
            var flowSize = flow.GetAutoSizeBounds();
            return new RectangleF(bounds.X, bounds.Y, flowSize.Width, flowSize.Height);
        }

        public override void Subscribe()
        {
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
            background.Subscribe();
            shadow.Subscribe();
            shadowAnchor.Subscribe();
            backgroundAnchor.Subscribe();
            text.Subscribe();
            description.Subscribe();
            flow.Subscribe();
            popupLayout.Subscribe();
        }

        public override void Unsubscribe()
        {
            shown = false;
            flow.Visible = false;
            hideTimer = 0;
            showTimer = 0;
            Canvas.RemoveComponent(this);
            Scene.Update -= Scene_Update;
            background.Unsubscribe();
            shadow.Unsubscribe();
            shadowAnchor.Unsubscribe();
            backgroundAnchor.Unsubscribe();
            text.Unsubscribe();
            description.Unsubscribe();
            flow.Unsubscribe();
            popupLayout.Unsubscribe();
        }

        public override void OnAdd()
        {
            base.OnAdd();
            Canvas.AddComponent(background);
            Canvas.AddComponent(shadow);
            Canvas.AddComponent(text);
            Canvas.AddComponent(description);
        }

        public override void OnRemove()
        {
            base.OnRemove();
            Canvas.RemoveComponent(background);
            Canvas.RemoveComponent(shadow);
            Canvas.RemoveComponent(text);
            Canvas.RemoveComponent(description);
        }

        private double showTimer = 0;
        private double hideTimer = 0;

        private void Scene_Update(double deltaTime)
        {
            if (autoSize)
            {
                bounds = GetAutoSizeBounds();
            }

            background.Color = Color.Background;
            text.Color = Color.Text;
            description.Color = Color.TextSecondary;

            if (popupLayout.Target != null)
            {
                bool over = popupLayout.Target.MouseOver;
                if (over)
                {
                    hideTimer = 0;
                } else
                {
                    showTimer = 0;
                }

                if (over && !shown)
                {
                    showTimer += deltaTime;
                    if (showTimer > showDelay)
                    {
                        showTimer = 0;
                        shown = true;
                        flow.Visible = true;
                        Canvas.AddComponent(this);
                    }
                }
                else if (!over && shown)
                {
                    hideTimer += deltaTime;
                    if (hideTimer > hideDelay)
                    {
                        hideTimer = 0;
                        shown = false;
                        flow.Visible = false;
                        Canvas.RemoveComponent(this);
                    }
                }
            }
        }
    }
}
