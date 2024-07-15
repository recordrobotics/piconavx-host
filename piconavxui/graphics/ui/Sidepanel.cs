using FontStashSharp;
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
    public class Sidepanel : UIController
    {
        public Sidepanel(string header, Canvas canvas) : base(canvas)
        {
            Header = header;
            background = new Image(canvas);
            canvas.AddComponent(background);
            background.ZIndex = ZIndex; // background
            background.Color = new Rgba32(10, 10, 10, 200);

            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            this.header = new Label(Header, canvas);
            canvas.AddComponent(this.header);
            this.header.FontSize = 18;
            this.header.ZIndex = ContentZIndex;
            this.header.Color = FSColor.White;
            headerAnchor = new AnchorLayout(this.header, this);
            headerAnchor.Anchor = Anchor.TopLeft | Anchor.Right;
            headerAnchor.Insets = new Insets(0, 20, 0, 0);
            headerAnchor.AllowResize = false; // instead of stretching, center it

            thisAnchor = new AnchorLayout(this);
            thisAnchor.Anchor = Anchor.TopLeft | Anchor.Bottom;
            thisAnchor.Insets = new Insets(0);
            bounds = new RectangleF(0, 0, 800, 0);
        }

        private AnchorLayout thisAnchor;
        private Image background;
        private Label header;
        private AnchorLayout backgroundAnchor;
        private AnchorLayout headerAnchor;

        public string Header { get; set; }

        private int zIndex = 0;
        public override int ZIndex
        {
            get => zIndex; set
            {
                zIndex = value;
                background.ZIndex = zIndex;
                header.ZIndex = ContentZIndex;
                Canvas.InvalidateHierarchy();
            }
        }

        public int ContentZIndex => zIndex + 1;

        private RectangleF bounds;
        public override RectangleF Bounds { get => bounds; set => bounds = value; }

        public override bool IsRenderable => false;

        public override void Subscribe()
        {
            background.Subscribe();
            header.Subscribe();
            thisAnchor.Subscribe();
            backgroundAnchor.Subscribe();
            headerAnchor.Subscribe();
        }

        public override void Unsubscribe()
        {
            background.Unsubscribe();
            header.Unsubscribe();
            thisAnchor.Unsubscribe();
            backgroundAnchor.Unsubscribe();
            headerAnchor.Unsubscribe();
        }

        public override void OnRemove()
        {
            base.OnRemove();
            Canvas.RemoveComponent(background);
            Canvas.RemoveComponent(header);
        }
    }
}
