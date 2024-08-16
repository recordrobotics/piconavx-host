using FontStashSharp;
using piconavx.ui.controllers;
using System.Drawing;
using static piconavx.ui.graphics.ui.Button;

namespace piconavx.ui.graphics.ui
{
    public class Badge : UIController
    {
        public Badge(string text, Canvas canvas) : base(canvas)
        {
            background = new Image(canvas);
            background.Transform = Transform;
            background.ZIndex = ZIndex; // background
            background.HitTestAlphaClip = 0.9f;
            background.Color = Color.Background;
            background.Texture = Texture.Pill;
            background.ImageType = ImageType.Sliced;
            background.Size = new Size((int)(bounds.Height / 2f), (int)(bounds.Height / 2f));
            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);
            background.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, NotifyClick);

            RaycastTransparency = RaycastTransparency.Transparent;

            this.text = new Label(text, canvas);
            this.text.Transform = Transform;
            this.text.FontSize = 13;
            this.text.ZIndex = ContentZIndex;
            this.text.Color = Color.Text;
            textAnchor = new AnchorLayout(this.text, this);
            textAnchor.Anchor = Anchor.TopLeft | Anchor.Bottom;
            textAnchor.Insets = padding;
            textAnchor.AllowResize = false; // instead of stretching, center it

            bounds = GetAutoSizeBounds();
        }

        private Image background;
        private Label text;
        private AnchorLayout backgroundAnchor;
        private AnchorLayout textAnchor;

        public ButtonColor Color { get; set; } = Theme.Primary;

        public string Text { get => this.text.Text; set => this.text.Text = value; }
        public float FontSize { get => this.text.FontSize; set => this.text.FontSize = value; }

        private bool autoSize = true;
        public bool AutoSize { get => autoSize; set => autoSize = value; }

        private int zIndex = 0;
        public override int ZIndex
        {
            get => zIndex; set
            {
                zIndex = value;
                background.ZIndex = zIndex;
                text.ZIndex = ContentZIndex;
                Canvas.InvalidateHierarchy();
            }
        }

        public int ContentZIndex => zIndex + 1;

        private RectangleF bounds;
        public override RectangleF Bounds
        {
            get => bounds; set
            {
                bounds = value;
                background.Size = new Size((int)(bounds.Height / 2f), (int)(bounds.Height / 2f));
            }
        }


        private Insets padding = new Insets(15, 4.5f, 15, 4.5f);
        public Insets Padding
        {
            get => padding; set
            {
                padding = value;
                textAnchor.Insets = new Insets(padding.Left, padding.Top, padding.Right, padding.Bottom);
            }
        }

        public override RaycastTransparency RaycastTransparency { get => base.RaycastTransparency; set => base.RaycastTransparency = background.RaycastTransparency = value; }
        public override bool IsRenderable => false;
        public override bool MouseDown { get => background.MouseDown; set => background.MouseDown = value; }
        public override bool MouseOver { get => background.MouseOver; set => background.MouseOver = value; }

        public RectangleF GetAutoSizeBounds()
        {
            var textSize = text.GetAutoSizeBounds();
            return new RectangleF(bounds.X, bounds.Y, textSize.Width + padding.Horizontal, textSize.Height + padding.Vertical);
        }

        public override void Subscribe()
        {
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
            background.Subscribe();
            text.Subscribe();
            backgroundAnchor.Subscribe();
            textAnchor.Subscribe();
        }

        public override void Unsubscribe()
        {
            Scene.Update -= Scene_Update;
            background.Unsubscribe();
            text.Unsubscribe();
            backgroundAnchor.Unsubscribe();
            textAnchor.Unsubscribe();
        }

        public override void OnAdd()
        {
            base.OnAdd();
            Canvas.AddComponent(background);
            Canvas.AddComponent(text);
        }

        public override void OnRemove()
        {
            base.OnRemove();
            Canvas.RemoveComponent(background);
            Canvas.RemoveComponent(text);
        }

        private void Scene_Update(double deltaTime)
        {
            if (autoSize)
            {
                bounds = GetAutoSizeBounds();
                background.Size = new Size((int)(bounds.Height/2f), (int)(bounds.Height / 2f));
            }

            background.Transform = Transform;
            text.Transform = Transform;

            background.Color = Color.Background;
            text.Color = Color.Text;
        }
    }
}
