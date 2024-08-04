using FontStashSharp;
using piconavx.ui.controllers;
using SixLabors.ImageSharp.PixelFormats;
using System.Drawing;

namespace piconavx.ui.graphics.ui
{
    public class ClientCard : FlowPanel
    {
        public readonly Rgba32 BACKGROUND = Rgba32.ParseHex("#262626");
        public readonly Rgba32 HEADER = Rgba32.ParseHex("#fff");
        public readonly Rgba32 TEXT = Rgba32.ParseHex("#BCBCBC");
        public readonly Rgba32 TEXT_SECONDARY = Rgba32.ParseHex("#6C6C6C");

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

        public string Id { get => idLabel.Text; set => idLabel.Text = value; }
        public string Address { get => addressLabel.Text; set => addressLabel.Text = value; }
        public string Port { get => portLabel.Text; set => portLabel.Text = value; }
        public string Version { get => versionLabel.Text; set => versionLabel.Text = value; }
        public string Status { get => statusLabel.Text; set => statusLabel.Text = value; }
        public string Memory { get => memoryLabel.Text; set => memoryLabel.Text = value; }
        public string Temperature { get => temperatureLabel.Text; set => temperatureLabel.Text = value; }

        private bool highBandwidth = false;
        public bool HighBandwidth
        {
            get => highBandwidth; set
            {
                if (!highBandwidth && value)
                {
                    Canvas.AddComponent(idBadge);
                }
                else if (highBandwidth && !value)
                {
                    Canvas.RemoveComponent(idBadge);
                }
                highBandwidth = value;
            }
        }

        public ClientCard(string id, bool highBandwidth, string address, string port, string version, string status, string memory, string temperature, Canvas canvas) : base(canvas)
        {
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
            }

            if (cardShadowTexture == null)
            {
                cardShadowTexture = Scene.AddResource(new Texture("assets/textures/cardshadow.png")
                {
                    Border = new Insets(32),
                    WrapMode = TextureWrapMode.Clamp
                });
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
            background.HitTestAlphaClip = 0.9f;
            background.RaycastTransparency = RaycastTransparency.Transparent;
            background.Color = BACKGROUND;
            background.Texture = cardBackgroundTexture;
            background.ImageType = ImageType.Sliced;
            background.Size = new Size(40, 40);
            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            shadow = new Image(canvas);
            shadow.RaycastTransparency = RaycastTransparency.Hidden;
            shadow.Color = new Rgba32(0, 0, 0, 128);
            shadow.Texture = cardShadowTexture;
            shadow.ImageType = ImageType.Sliced;
            shadow.Size = new Size(80, 80);
            shadowAnchor = new AnchorLayout(shadow, this);
            shadowAnchor.Anchor = Anchor.All;
            shadowAnchor.Insets = new Insets(-shadow.Size.Width + background.Size.Width, 8 - shadow.Size.Height + background.Size.Height, -shadow.Size.Width + background.Size.Width, -8 - shadow.Size.Height + background.Size.Height);

            thumbnail = new Image(canvas);
            thumbnail.RaycastTransparency = RaycastTransparency.Hidden;
            thumbnail.Color = new Rgba32(255, 255, 255, 255);
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
            thumbnailShadow.RaycastTransparency = RaycastTransparency.Hidden;
            thumbnailShadow.Color = new Rgba32(0, 0, 0, 230);
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
            idLabel.FontSize = 20;
            idLabel.Color = new FSColor(HEADER.ToVector4());
            idLabel.Font = FontFace.InterBold;
            idLabel.RenderOffset = new System.Numerics.Vector2(0, 3);
            row1.Components.Add(idLabel);

            idBadge = new Badge("High Bandwidth", canvas);
            row1.Components.Add(idBadge);
            idBadgeTooltip = new Tooltip("This client is currently in high bandwidth mode.", "High Bandwidth mode groups data packets together\ninto one to reduce the processing overhead.", idBadge, canvas);
            idBadgeTooltip.Anchor = PopupAnchor.Right;

            addressRow = new FlowPanel(canvas);
            versionRow = new FlowPanel(canvas);
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
            addressLabel.FontSize = 14;
            addressLabel.Color = new FSColor(TEXT.ToVector4());
            addressRow.Components.Add(addressLabel);
            addressSeparatorLabel = new Label(":", canvas);
            addressSeparatorLabel.FontSize = 14;
            addressSeparatorLabel.Color = new FSColor(TEXT_SECONDARY.ToVector4());
            addressRow.Components.Add(addressSeparatorLabel);
            portLabel = new Label(port, canvas);
            portLabel.FontSize = 14;
            portLabel.Color = new FSColor(TEXT_SECONDARY.ToVector4());
            addressRow.Components.Add(portLabel);

            versionPrefixLabel = new Label("v", canvas);
            versionPrefixLabel.FontSize = 14;
            versionPrefixLabel.Color = new FSColor(TEXT_SECONDARY.ToVector4());
            versionRow.Components.Add(versionPrefixLabel);
            versionLabel = new Label(version, canvas);
            versionLabel.FontSize = 14;
            versionLabel.Color = new FSColor(TEXT.ToVector4());
            versionRow.Components.Add(versionLabel);

            statusIcon = new Image(canvas);
            statusIcon.Bounds = new RectangleF(0, 0, 36, 36);
            statusIcon.Texture = calibrateTexture;
            statusIcon.Color = TEXT_SECONDARY;
            statusIcon.RaycastTransparency = RaycastTransparency.Hidden;
            row3.Components.Add(statusIcon);
            statusLabel = new Label(status, canvas);
            statusLabel.FontSize = 14;
            statusLabel.Color = new FSColor(TEXT.ToVector4());
            row3.Components.Add(statusLabel);

            memoryIcon = new Image(canvas);
            memoryIcon.Bounds = new RectangleF(0, 0, 36, 36);
            memoryIcon.Texture = memoryTexture;
            memoryIcon.Color = TEXT_SECONDARY;
            memoryIcon.RaycastTransparency = RaycastTransparency.Hidden;
            row4.Components.Add(memoryIcon);
            memoryLabel = new Label(memory, canvas);
            memoryLabel.FontSize = 14;
            memoryLabel.Color = new FSColor(TEXT.ToVector4());
            row4.Components.Add(memoryLabel);

            temperatureIcon = new Image(canvas);
            temperatureIcon.Bounds = new RectangleF(0, 0, 36, 36);
            temperatureIcon.Texture = temperatureTexture;
            temperatureIcon.Color = TEXT_SECONDARY;
            temperatureIcon.RaycastTransparency = RaycastTransparency.Hidden;
            row5.Components.Add(temperatureIcon);
            temperatureLabel = new Label(temperature, canvas);
            temperatureLabel.FontSize = 14;
            temperatureLabel.Color = new FSColor(TEXT.ToVector4());
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
    }
}
