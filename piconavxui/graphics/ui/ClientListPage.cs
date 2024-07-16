using piconavx.ui.controllers;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public class ClientListPage : Page
    {
        private Image background;
        private AnchorLayout backgroundAnchor;

        private Panel headerPanel;
        private AnchorLayout headerPanelLayout;

        private Label header;
        private AnchorLayout headerLayout;

        private FlowPanel controlPanel;
        private AnchorLayout controlPanelLayout;

        public readonly Rgba32 BACKGROUND = Rgba32.ParseHex("#0F0F0F");
        public readonly Rgba32 TEXT = Rgba32.ParseHex("#FFF");

        public ClientListPage(Canvas canvas) : base(canvas)
        {
            background = new Image(canvas);
            background.Color = BACKGROUND;
            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            headerPanel = new Panel(canvas);
            headerPanel.Bounds = new RectangleF(0, 0, 0, 93);
            headerPanelLayout = new AnchorLayout(headerPanel, this);
            headerPanelLayout.Anchor = Anchor.TopLeft | Anchor.Right;
            headerPanelLayout.Insets = new Insets(35, 0, 17, 0);

            header = new Label("Connected Clients", canvas);
            header.FontSize = 27;
            header.Color = new FontStashSharp.FSColor(TEXT.ToVector4());
            headerLayout = new AnchorLayout(header, headerPanel);
            headerLayout.Anchor = Anchor.TopLeft | Anchor.Bottom;
            headerLayout.AllowResize = false;
            headerLayout.Insets = new Insets(0);

            UpdateZIndex();
        }

        private int zIndex = 0;
        public override int ZIndex
        {
            get => zIndex; set
            {
                zIndex = value;
                UpdateZIndex();
            }
        }

        private RectangleF bounds = new RectangleF(0, 0, 0, 0);
        public override RectangleF Bounds { get => bounds; set => bounds = value; }

        private void UpdateZIndex()
        {
            background.ZIndex = zIndex;
            header.ZIndex = zIndex + 1;
        }

        public override void Show()
        {
            SubscribeLater(
                background, backgroundAnchor,
                headerPanel, headerPanelLayout,
                header, headerLayout
                );

            Canvas.AddComponent(background);
            Canvas.AddComponent(headerPanel);
            Canvas.AddComponent(header);
        }

        public override void Hide()
        {
            Canvas.RemoveComponent(background);
            Canvas.RemoveComponent(headerPanel);
            Canvas.RemoveComponent(header);
            UnsubscribeLater(
                background, backgroundAnchor,
                headerPanel, headerPanelLayout,
                header, headerLayout
                );
        }
    }
}
