using piconavx.ui.controllers;
using System.Drawing;
using System.Numerics;
using static piconavx.ui.graphics.ui.Button;

namespace piconavx.ui.graphics.ui
{
    public class Alert : UIController
    {
        private static Texture? cardShadowTexture;

        private static void _alertOneShot(Alert alert)
        {
            alert.OneShot = true;
            alert.Subscribe();
            alert.Show();
        }

        public static Alert CreateOneShot(string text, string description, Canvas canvas)
        {
            var alert = new Alert(text, description, canvas);

            if (Scene.InEvent)
            {
                Scene.InvokeLater(() => _alertOneShot(alert), DeferralMode.NextEvent);
            }
            else
            {
                _alertOneShot(alert);
            }

            return alert;
        }

        public Alert(string text, string description, Canvas canvas) : base(canvas)
        {
            cardShadowTexture ??= Scene.AddResource(new Texture("assets/textures/cardshadow.png")
            {
                Border = new Insets(32),
                WrapMode = TextureWrapMode.Clamp
            });

            StackCount++;
            zIndex += StackIndex += StackIncr;

            popupLayout = new PopupLayout(this);
            popupLayout.Anchor = PopupAnchor.Bottom;
            popupLayout.Offset = new Vector2(0, 40);

            background = new Image(canvas);
            background.Transform = Transform;
            background.HitTestAlphaClip = 0.9f;
            background.Color = Color.Background;
            background.Texture = Texture.RoundedRect;
            background.ImageType = ImageType.Sliced;
            background.Size = new Size(20, 20);
            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = controllers.Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            shadow = new Image(canvas);
            shadow.Transform = Transform;
            shadow.RaycastTransparency = RaycastTransparency.Hidden;
            shadow.Color = Theme.CardShadow;
            shadow.Texture = cardShadowTexture;
            shadow.ImageType = ImageType.Sliced;
            shadow.Size = new Size(80, 80);
            shadowAnchor = new AnchorLayout(shadow, this);
            shadowAnchor.Anchor = controllers.Anchor.All;
            shadowAnchor.Insets = new Insets(-shadow.Size.Width + background.Size.Width, 8 - shadow.Size.Height + background.Size.Height, -shadow.Size.Width + background.Size.Width, -8 - shadow.Size.Height + background.Size.Height);

            RaycastTransparency = RaycastTransparency.Transparent;

            this.text = new Label(text, canvas);
            this.text.Transform = Transform;
            this.text.FontSize = 18;
            this.text.Color = Color.Text;

            this.description = new Label(description, canvas);
            this.description.Transform = Transform;
            this.description.FontSize = 14;
            this.description.Color = Color.TextSecondary;

            flow = new FlowLayout(this);
            flow.Direction = FlowDirection.Vertical;
            flow.Padding = padding;
            flow.Gap = 20;
            flow.Components.Add(this.text);
            flow.Components.Add(this.description);

            bounds = GetAutoSizeBounds();

            if (Canvas.InEvent)
            {
                Scene.InvokeLater(UpdateZIndex, DeferralMode.NextEvent);
            } else
            {
                UpdateZIndex();
            }
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

        const int StackIncr = 3;

        private static int StackCount = 0;
        private static int StackIndex = 0;

        private int zIndex = 60;
        public override int ZIndex
        {
            get => zIndex; set
            {
                zIndex = value;
                UpdateZIndex();
            }
        }

        public void UpdateZIndex()
        {
            background.ZIndex = zIndex + 1;
            shadow.ZIndex = zIndex;
            text.ZIndex = ContentZIndex;
            description.ZIndex = ContentZIndex;
            Canvas.InvalidateHierarchy();
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


        private Insets padding = new Insets(60, 40, 60, 40);
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

        private double showDuration = 2;
        public double ShowDuration { get => showDuration; set => showDuration = value; }

        private bool oneShot = false;
        public bool OneShot { get => oneShot; set => oneShot = value; }

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
            timer = 0;
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

            if(--StackCount == 0)
            {
                StackIndex = 0;
            }
        }

        public void Show()
        {
            if (Scene.InEvent)
            {
                Scene.InvokeLater(Show, DeferralMode.NextEvent);
            }
            else
            {
                timer = 0;

                if (shown)
                    return;

                shown = true;
                flow.Visible = true;
                Canvas.AddComponent(this);
            }
        }

        private double timer = 0;
        private Transition<float> entranceTransition = new(100, 0.05);

        private void Scene_Update(double deltaTime)
        {
            if (autoSize)
            {
                bounds = GetAutoSizeBounds();
            }

            background.Color = Color.Background;
            text.Color = Color.Text;
            description.Color = Color.TextSecondary;

            if (shown)
            {
                timer += deltaTime;
                if (timer > showDuration)
                {
                    timer = 0;
                    shown = false;
                    entranceTransition.Value = 100;
                }
                else
                {
                    entranceTransition.Value = 0;
                }
            }
            else
            {
                entranceTransition.Value = 100;
            }

            entranceTransition.Step(deltaTime);
            if (!shown && entranceTransition.Reached)
            {
                flow.Visible = false;
                Canvas.RemoveComponent(this);

                if (oneShot)
                {
                    Scene.InvokeLater(Unsubscribe, DeferralMode.NextEvent);
                }
            }

            Transform.UpdateCache();
            Transform.Position = new Vector3(0, entranceTransition.Value / 100f * (Bounds.Height + popupLayout.Offset.Y), 0);
        }
    }
}
