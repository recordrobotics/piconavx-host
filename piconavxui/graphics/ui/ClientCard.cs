using piconavx.ui.controllers;
using System.Drawing;
using System.Numerics;

namespace piconavx.ui.graphics.ui
{
    public class ClientCard : FlowPanel
    {
        private static Texture? cardBackgroundTexture;
        private static Texture? cardMaskTexture;
        private static Texture? cardShadowTexture;
        private static Texture? thumbnailTexture;
        private static Texture? calibrateTexture;
        private static Texture? memoryTexture;
        private static Texture? temperatureTexture;

        private Image background;
        private AnchorLayout backgroundAnchor;
        private Image shadow;
        private AnchorLayout shadowAnchor;

        private Image thumbnail;
        private AnchorLayout thumbnailAnchor;
        private Image thumbnailShadow;
        private AnchorLayout thumbnailShadowAnchor;

        private FlowPanel row1;
        private FlowPanel row2;
        private FlowPanel row3;
        private FlowPanel row4;
        private FlowPanel row5;
        private AnchorLayout row1Anchor;
        private AnchorLayout row2Anchor;

        private Tooltip addressRowTooltip;
        private Tooltip row3Tooltip;
        private Tooltip row4Tooltip;
        private Tooltip row5Tooltip;

        private Label idLabel;
        private Badge idBadge;
        private Tooltip idBadgeTooltip;

        private FlowPanel addressRow;
        private FlowPanel versionRow;

        private Label addressLabel;
        private Label addressSeparatorLabel;
        private Label portLabel;

        private Label versionPrefixLabel;
        private Label versionLabel;

        private Image statusIcon;
        private Label statusLabel;

        private Image memoryIcon;
        private Label memoryLabel;

        private Image temperatureIcon;
        private Label temperatureLabel;

        public override bool MouseOver { get => background.MouseOver || addressRow.MouseOver || row3.MouseOver || row4.MouseOver || row5.MouseOver || idBadge.MouseOver; set => _ = value; }
        public override bool MouseDown { get => background.MouseDown || addressRow.MouseDown || row3.MouseDown || row4.MouseDown || row5.MouseDown || idBadge.MouseDown; set => _ = value; }

        public string Id { get => idLabel.Text; set => idLabel.Text = value; }
        public string Address { get => addressLabel.Text; set => addressLabel.Text = value; }
        public string Port { get => portLabel.Text; set => portLabel.Text = value; }
        public string Version { get => versionLabel.Text; set => versionLabel.Text = value; }
        public string Status { get => statusLabel.Text; set => statusLabel.Text = value; }
        public string Memory { get => memoryLabel.Text; set => memoryLabel.Text = value; }
        public string Temperature { get => temperatureLabel.Text; set => temperatureLabel.Text = value; }

        private bool highBandwidth = false;
        private bool highBandwidthBadgeCreated = false;
        public bool HighBandwidth
        {
            get => highBandwidth; set
            {
                if (!highBandwidth && value && highBandwidthBadgeCreated)
                {
                    Canvas.AddComponent(idBadge);
                }
                else if (highBandwidth && !value && highBandwidthBadgeCreated)
                {
                    Canvas.RemoveComponent(idBadge);
                }
                highBandwidth = value;
            }
        }

        public ClientCard(Canvas canvas) : this(string.Empty, false, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, canvas)
        {
        }

        public ClientCard(string id, bool highBandwidth, string address, string port, string version, string status, string memory, string temperature, Canvas canvas) : base(canvas)
        {
            Transform.SetOrigin(this, new(0.5f));

            SupportsInputEvents = true;
            this.highBandwidth = highBandwidth;

            AutoSize = FlowLayout.AutoSize.Y;
            Bounds = new RectangleF(0, 0, 521, 0);
            Padding = new Insets(30, 330, 30, 30);
            Gap = 15;

            if (cardBackgroundTexture == null)
            {
                cardBackgroundTexture = Scene.AddResource(new Texture("assets/textures/card.png")
                {
                    Border = new Insets(24),
                    WrapMode = TextureWrapMode.Clamp
                });

                cardBackgroundTexture.UpdateSettings();
            }

            if (cardShadowTexture == null)
            {
                cardShadowTexture = Scene.AddResource(new Texture("assets/textures/cardshadow.png")
                {
                    Border = new Insets(32),
                    WrapMode = TextureWrapMode.Clamp
                });

                cardShadowTexture.UpdateSettings();
            }

            if (cardMaskTexture == null)
            {
                cardMaskTexture = Scene.AddResource(new Texture("assets/textures/cardmask.png")
                {
                    Border = new Insets(24),
                    WrapMode = TextureWrapMode.Clamp
                });

                cardMaskTexture.UpdateSettings();
            }

            thumbnailTexture ??= Scene.AddResource(new Texture("assets/textures/brand.jpg"));

            calibrateTexture ??= Scene.AddResource(new Texture("assets/textures/calibrate.png"));
            memoryTexture ??= Scene.AddResource(new Texture("assets/textures/memory.png"));
            temperatureTexture ??= Scene.AddResource(new Texture("assets/textures/temperature.png"));

            background = new Image(canvas);
            background.Transform = Transform;
            background.HitTestAlphaClip = 0.9f;
            background.RaycastTransparency = RaycastTransparency.Transparent;
            background.Color = Theme.CardBackground;
            background.Texture = cardBackgroundTexture;
            background.ImageType = ImageType.Sliced;
            background.Size = new Size(40, 40);
            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            shadow = new Image(canvas);
            shadow.Transform = Transform;
            shadow.RaycastTransparency = RaycastTransparency.Hidden;
            shadow.Color = Theme.CardShadow;
            shadow.Texture = cardShadowTexture;
            shadow.ImageType = ImageType.Sliced;
            shadow.Size = new Size(80, 80);
            shadowAnchor = new AnchorLayout(shadow, this);
            shadowAnchor.Anchor = Anchor.All;
            shadowAnchor.Insets = new Insets(-shadow.Size.Width + background.Size.Width, 8 - shadow.Size.Height + background.Size.Height, -shadow.Size.Width + background.Size.Width, -8 - shadow.Size.Height + background.Size.Height);

            thumbnail = new Image(canvas);
            thumbnail.Transform = Transform;
            thumbnail.RaycastTransparency = RaycastTransparency.Hidden;
            thumbnail.Color = Theme.CardThumbnail;
            thumbnail.Texture = cardMaskTexture;
            thumbnail.Mask = thumbnailTexture;
            thumbnail.PreserveAspect = true;
            thumbnail.ImageType = ImageType.Sliced;
            thumbnail.Size = new Size(40, 40);
            thumbnail.Bounds = new RectangleF(0, 0, 0, 300);
            thumbnailAnchor = new AnchorLayout(thumbnail, this);
            thumbnailAnchor.Anchor = Anchor.TopLeft | Anchor.Right;
            thumbnailAnchor.Insets = new Insets(0);

            thumbnailShadow = new Image(canvas);
            thumbnailShadow.Transform = Transform;
            thumbnailShadow.RaycastTransparency = RaycastTransparency.Hidden;
            thumbnailShadow.Color = Theme.CardThumbnailShadow;
            thumbnailShadow.Texture = cardShadowTexture;
            thumbnailShadow.ImageType = ImageType.Sliced;
            thumbnailShadow.Size = new Size(55, 55);
            thumbnailShadowAnchor = new AnchorLayout(thumbnailShadow, thumbnail);
            thumbnailShadowAnchor.Anchor = Anchor.All;
            thumbnailShadowAnchor.Insets = new Insets(-thumbnailShadow.Size.Width + thumbnail.Size.Width, 8 - thumbnailShadow.Size.Height + thumbnail.Size.Height, -thumbnailShadow.Size.Width + thumbnail.Size.Width, -8 - thumbnailShadow.Size.Height + thumbnail.Size.Height);

            row1 = new FlowPanel(canvas);
            row2 = new FlowPanel(canvas);
            row3 = new FlowPanel(canvas);
            row4 = new FlowPanel(canvas);
            row5 = new FlowPanel(canvas);
            row1.Transform = Transform;
            row2.Transform = Transform;
            row3.Transform = Transform;
            row4.Transform = Transform;
            row5.Transform = Transform;
            Components.Add(row1);
            Components.Add(row2);
            Components.Add(row3);
            Components.Add(row4);
            Components.Add(row5);
            row1.Gap = 4;
            row2.Gap = 4;
            row3.Gap = 4;
            row4.Gap = 4;
            row5.Gap = 4;
            row1.Direction = FlowDirection.Horizontal;
            row2.Direction = FlowDirection.Horizontal;
            row3.Direction = FlowDirection.Horizontal;
            row4.Direction = FlowDirection.Horizontal;
            row5.Direction = FlowDirection.Horizontal;
            row1.AlignItems = AlignItems.Middle;
            row2.AlignItems = AlignItems.Middle;
            row3.AlignItems = AlignItems.Middle;
            row4.AlignItems = AlignItems.Middle;
            row5.AlignItems = AlignItems.Middle;
            row1.Stretch = true;
            row2.Stretch = true;
            row1.AutoSize = FlowLayout.AutoSize.Y;
            row2.AutoSize = FlowLayout.AutoSize.Y;
            row1Anchor = new AnchorLayout(row1, this);
            row2Anchor = new AnchorLayout(row2, this);
            row1Anchor.Anchor = Anchor.Left | Anchor.Right;
            row2Anchor.Anchor = Anchor.Left | Anchor.Right;
            row1Anchor.Insets = Padding;
            row2Anchor.Insets = Padding;
            row3.Padding = new Insets(0, 7.5f, 0, 0);

            idLabel = new Label(id, canvas);
            idLabel.Transform = Transform;
            idLabel.FontSize = 20;
            idLabel.Color = Theme.Header;
            idLabel.Font = FontFace.InterBold;
            idLabel.RenderOffset = new System.Numerics.Vector2(0, 3);
            row1.Components.Add(idLabel);

            idBadge = new Badge("High Bandwidth", canvas);
            idBadge.Transform = Transform;
            row1.Components.Add(idBadge);
            idBadgeTooltip = new Tooltip("This client is currently in high bandwidth mode.", "High Bandwidth mode groups data packets together\ninto one to reduce the processing overhead.", idBadge, canvas);
            idBadgeTooltip.Anchor = PopupAnchor.Right;

            addressRow = new FlowPanel(canvas);
            addressRow.Transform = Transform;
            versionRow = new FlowPanel(canvas);
            versionRow.Transform = Transform;
            addressRow.Direction = FlowDirection.Horizontal;
            versionRow.Direction = FlowDirection.Horizontal;
            addressRow.AlignItems = AlignItems.Middle;
            versionRow.AlignItems = AlignItems.Middle;
            row2.Components.Add(addressRow);
            row2.Components.Add(versionRow);

            addressRow.SupportsInputEvents = true;
            addressRow.RaycastTransparency = RaycastTransparency.Opaque;
            row3.SupportsInputEvents = true;
            row3.RaycastTransparency = RaycastTransparency.Opaque;
            row4.SupportsInputEvents = true;
            row4.RaycastTransparency = RaycastTransparency.Opaque;
            row5.SupportsInputEvents = true;
            row5.RaycastTransparency = RaycastTransparency.Opaque;

            addressRowTooltip = new Tooltip("Client IP address", string.Empty, addressRow, canvas)
            {
                Anchor = PopupAnchor.Right
            };
            row3Tooltip = new Tooltip("Sensor status", string.Empty, row3, canvas)
            {
                Anchor = PopupAnchor.Right
            };
            row4Tooltip = new Tooltip("Sensor memory usage", string.Empty, row4, canvas)
            {
                Anchor = PopupAnchor.Right
            };
            row5Tooltip = new Tooltip("Sensor temperatures (core | sensor)", string.Empty, row5, canvas)
            {
                Anchor = PopupAnchor.Right
            };

            addressLabel = new Label(address, canvas);
            addressLabel.Transform = Transform;
            addressLabel.FontSize = 14;
            addressLabel.Color = Theme.Text;
            addressRow.Components.Add(addressLabel);
            addressSeparatorLabel = new Label(":", canvas);
            addressSeparatorLabel.Transform = Transform;
            addressSeparatorLabel.FontSize = 14;
            addressSeparatorLabel.Color = Theme.TextSecondary;
            addressRow.Components.Add(addressSeparatorLabel);
            portLabel = new Label(port, canvas);
            portLabel.Transform = Transform;
            portLabel.FontSize = 14;
            portLabel.Color = Theme.TextSecondary;
            addressRow.Components.Add(portLabel);

            versionPrefixLabel = new Label("v", canvas);
            versionPrefixLabel.Transform = Transform;
            versionPrefixLabel.FontSize = 14;
            versionPrefixLabel.Color = Theme.TextSecondary;
            versionRow.Components.Add(versionPrefixLabel);
            versionLabel = new Label(version, canvas);
            versionLabel.Transform = Transform;
            versionLabel.FontSize = 14;
            versionLabel.Color = Theme.Text;
            versionRow.Components.Add(versionLabel);

            statusIcon = new Image(canvas);
            statusIcon.Transform = Transform;
            statusIcon.Bounds = new RectangleF(0, 0, 36, 36);
            statusIcon.Texture = calibrateTexture;
            statusIcon.Color = Theme.TextSecondary;
            statusIcon.RaycastTransparency = RaycastTransparency.Hidden;
            row3.Components.Add(statusIcon);
            statusLabel = new Label(status, canvas);
            statusLabel.Transform = Transform;
            statusLabel.FontSize = 14;
            statusLabel.Color = Theme.Text;
            row3.Components.Add(statusLabel);

            memoryIcon = new Image(canvas);
            memoryIcon.Transform = Transform;
            memoryIcon.Bounds = new RectangleF(0, 0, 36, 36);
            memoryIcon.Texture = memoryTexture;
            memoryIcon.Color = Theme.TextSecondary;
            memoryIcon.RaycastTransparency = RaycastTransparency.Hidden;
            row4.Components.Add(memoryIcon);
            memoryLabel = new Label(memory, canvas);
            memoryLabel.Transform = Transform;
            memoryLabel.FontSize = 14;
            memoryLabel.Color = Theme.Text;
            row4.Components.Add(memoryLabel);

            temperatureIcon = new Image(canvas);
            temperatureIcon.Transform = Transform;
            temperatureIcon.Bounds = new RectangleF(0, 0, 36, 36);
            temperatureIcon.Texture = temperatureTexture;
            temperatureIcon.Color = Theme.TextSecondary;
            temperatureIcon.RaycastTransparency = RaycastTransparency.Hidden;
            row5.Components.Add(temperatureIcon);
            temperatureLabel = new Label(temperature, canvas);
            temperatureLabel.Transform = Transform;
            temperatureLabel.FontSize = 14;
            temperatureLabel.Color = Theme.Text;
            row5.Components.Add(temperatureLabel);

            UpdateZIndex();
        }

        protected override void UpdateZIndex()
        {
            background.ZIndex = ZIndex + 1;

            shadow.ZIndex = ZIndex;

            thumbnail.ZIndex = ZIndex + 3;
            thumbnailShadow.ZIndex = ZIndex + 2;

            row1.ZIndex = ZIndex + 2;
            row2.ZIndex = ZIndex + 2;
            row3.ZIndex = ZIndex + 2;
            row4.ZIndex = ZIndex + 2;
            row5.ZIndex = ZIndex + 2;
            addressRow.ZIndex = ZIndex + 2;
            versionRow.ZIndex = ZIndex + 2;

            idLabel.ZIndex = ZIndex + 3;
            idBadge.ZIndex = ZIndex + 3;
            addressLabel.ZIndex = ZIndex + 3;
            addressSeparatorLabel.ZIndex = ZIndex + 3;
            portLabel.ZIndex = ZIndex + 3;
            versionPrefixLabel.ZIndex = ZIndex + 3;
            versionLabel.ZIndex = ZIndex + 3;
            statusIcon.ZIndex = ZIndex + 3;
            statusLabel.ZIndex = ZIndex + 3;
            memoryIcon.ZIndex = ZIndex + 3;
            memoryLabel.ZIndex = ZIndex + 3;
            temperatureIcon.ZIndex = ZIndex + 3;
            temperatureLabel.ZIndex = ZIndex + 3;
        }

        public override void Subscribe()
        {
            base.Subscribe();
            background.Subscribe();
            backgroundAnchor.Subscribe();
            shadow.Subscribe();
            shadowAnchor.Subscribe();
            thumbnail.Subscribe();
            thumbnailAnchor.Subscribe();
            thumbnailShadow.Subscribe();
            thumbnailShadowAnchor.Subscribe();
            row1Anchor.Subscribe();
            row2Anchor.Subscribe();
            idBadgeTooltip.Subscribe();
            addressRowTooltip.Subscribe();
            row3Tooltip.Subscribe();
            row4Tooltip.Subscribe();
            row5Tooltip.Subscribe();

            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.AfterGeneral, Scene_Update);
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            background.Unsubscribe();
            backgroundAnchor.Unsubscribe();
            shadow.Unsubscribe();
            shadowAnchor.Unsubscribe();
            thumbnail.Unsubscribe();
            thumbnailAnchor.Unsubscribe();
            thumbnailShadow.Unsubscribe();
            thumbnailShadowAnchor.Unsubscribe();
            row1Anchor.Unsubscribe();
            row2Anchor.Unsubscribe();
            idBadgeTooltip.Unsubscribe();
            addressRowTooltip.Unsubscribe();
            row3Tooltip.Unsubscribe();
            row4Tooltip.Unsubscribe();
            row5Tooltip.Unsubscribe();

            Scene.Update -= Scene_Update;
        }

        public override void OnAdd()
        {
            base.OnAdd();
            Canvas.AddComponent(background);
            Canvas.AddComponent(shadow);
            Canvas.AddComponent(thumbnail);
            Canvas.AddComponent(thumbnailShadow);
            Canvas.AddComponent(row1);
            Canvas.AddComponent(row2);
            Canvas.AddComponent(row3);
            Canvas.AddComponent(row4);
            Canvas.AddComponent(row5);
            Canvas.AddComponent(idLabel);
            highBandwidthBadgeCreated = true;
            if (highBandwidth)
            {
                Canvas.AddComponent(idBadge);
            }
            Canvas.AddComponent(addressRow);
            Canvas.AddComponent(versionRow);
            Canvas.AddComponent(addressLabel);
            Canvas.AddComponent(addressSeparatorLabel);
            Canvas.AddComponent(portLabel);
            Canvas.AddComponent(versionPrefixLabel);
            Canvas.AddComponent(versionLabel);
            Canvas.AddComponent(statusIcon);
            Canvas.AddComponent(statusLabel);
            Canvas.AddComponent(memoryIcon);
            Canvas.AddComponent(memoryLabel);
            Canvas.AddComponent(temperatureIcon);
            Canvas.AddComponent(temperatureLabel);
        }

        public override void OnRemove()
        {
            base.OnRemove();
            highBandwidthBadgeCreated = false;
            Canvas.RemoveComponent(background);
            Canvas.RemoveComponent(shadow);
            Canvas.RemoveComponent(thumbnail);
            Canvas.RemoveComponent(thumbnailShadow);
        }

        private Transition<float> scaleTransition = new(1, 0.05);

        private void Scene_Update(double delta)
        {
            scaleTransition.Value = MouseDown ? 0.95f : MouseOver ? 1.05f : 1f;
            scaleTransition.Step(delta);

            Transform.UpdateCache();
            Transform.Scale = new Vector3(scaleTransition.Value);
        }
    }
}
