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
    public class Sidepanel : Panel
    {
        public PrioritizedList<PrioritizedAction<GenericPriority>> BackClick = new();

        private static Texture? backIcon;

        public Sidepanel(string header, Canvas canvas) : base(canvas)
        {
            backIcon ??= Scene.AddResource(new Texture("assets/textures/back.png"));

            Header = header;
            background = new Image(canvas);
            background.Color = Theme.SidepanelBackground;

            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            var virtualHeader = new VirtualUIController(canvas)
            {
                GetBounds = () => new RectangleF(Bounds.X, Bounds.Y, Bounds.Width, 130)
            };

            this.header = new Label(Header, canvas);
            this.header.FontSize = 18;
            this.header.Color = Theme.Header;
            headerAnchor = new AnchorLayout(this.header, virtualHeader);
            headerAnchor.Anchor = Anchor.TopLeft | Anchor.Right | Anchor.Bottom;
            headerAnchor.Insets = new Insets(0);
            headerAnchor.AllowResize = false; // instead of stretching, center it

            backButton = new Button("Back", canvas);
            backButton.Padding = new Insets(16);
            backButton.IsIconButton = true;
            backButton.Icon = backIcon;
            backButton.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, NotifyBackClick);
            backButtonLayout = new AnchorLayout(backButton, virtualHeader);
            backButtonLayout.Anchor = Anchor.TopLeft | Anchor.Bottom;
            backButtonLayout.Insets = new Insets(20, 0, 0, 0);
            backButtonLayout.AllowResize = false;

            thisAnchor = new AnchorLayout(this);
            thisAnchor.Anchor = Anchor.TopLeft | Anchor.Bottom;
            thisAnchor.Insets = new Insets(0);
            Bounds = new RectangleF(0, 0, 800, 0);

            UpdateZIndex();
        }

        private AnchorLayout thisAnchor;
        private Image background;
        private Label header;
        private AnchorLayout backgroundAnchor;
        private AnchorLayout headerAnchor;
        private Button backButton;
        private AnchorLayout backButtonLayout;

        public string Header { get; set; }

        protected override void UpdateZIndex()
        {
            background.ZIndex = ZIndex;
            header.ZIndex = ContentZIndex;
            backButton.ZIndex = ContentZIndex;
        }

        public int ContentZIndex => ZIndex + 1;

        public override void Subscribe()
        {
            background.Subscribe();
            header.Subscribe();
            backButton.Subscribe();
            thisAnchor.Subscribe();
            backgroundAnchor.Subscribe();
            headerAnchor.Subscribe();
            backButtonLayout.Subscribe();
        }

        public override void Unsubscribe()
        {
            background.Unsubscribe();
            header.Unsubscribe();
            backButton.Unsubscribe();
            thisAnchor.Unsubscribe();
            backgroundAnchor.Unsubscribe();
            headerAnchor.Unsubscribe();
            backButtonLayout.Unsubscribe();
        }

        public override void OnAdd()
        {
            base.OnAdd();
            Canvas.AddComponent(background);
            Canvas.AddComponent(header);
            Canvas.AddComponent(backButton);
        }

        public override void OnRemove()
        {
            base.OnRemove();
            Canvas.RemoveComponent(background);
            Canvas.RemoveComponent(header);
            Canvas.RemoveComponent(backButton);
        }

        public void NotifyBackClick()
        {
            foreach (var action in BackClick)
            {
                action.Action.Invoke();
            }
        }
    }
}
