using piconavx.ui.controllers;
using System.Drawing;

namespace piconavx.ui.graphics.ui
{
    public class StatusIndicator : FlowPanel
    {
        public StatusIndicator(string text, bool status, Canvas canvas) : base(canvas)
        {
            Direction = FlowDirection.Horizontal;
            Gap = 10;
            AlignItems = AlignItems.Middle;

            this.status = status;

            img = new Image(canvas);
            img.Transform = Transform;
            img.HitTestAlphaClip = 0.9f;
            img.Color = status ? Theme.StatusOk : Theme.StatusError;
            img.Texture = Texture.Pill;
            img.Bounds = new RectangleF(0, 0, 23, 23);
            Components.Add(img);

            RaycastTransparency = RaycastTransparency.Hidden;

            this.text = new Label(text, canvas);
            this.text.Transform = Transform;
            this.text.FontSize = 12.5f;
            this.text.Color = Theme.Text;
            Components.Add(this.text);

            UpdateZIndex();
        }

        public override RaycastTransparency RaycastTransparency { get => base.RaycastTransparency; set => base.RaycastTransparency = img.RaycastTransparency = value; }

        public StatusIndicator(string text, Func<bool> statusDelegate, Canvas canvas):this(text, statusDelegate.Invoke(), canvas)
        {
            this.statusFunc = statusDelegate;
        }

        protected override void UpdateZIndex()
        {
            img.ZIndex = ZIndex;
            this.text.ZIndex = ZIndex;
        }

        private Image img;
        private Label text;

        public UIColor Color { get => this.text.Color; set => this.text.Color = value; }

        public string Text { get => this.text.Text; set => this.text.Text = value; }

        private Func<bool>? statusFunc;
        public Func<bool>? StatusDelegate { get => statusFunc; set => statusFunc = value; }

        public float FontSize { get => this.text.FontSize; set => this.text.FontSize = value; }

        private bool status = false;
        public bool Status { get => status; set => status = value; }

        public override void Subscribe()
        {
            base.Subscribe();
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.General, Scene_Update);
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            Scene.Update -= Scene_Update;
        }

        public override void OnAdd()
        {
            base.OnAdd();
            Canvas.AddComponent(img);
            Canvas.AddComponent(text);
        }

        private void Scene_Update(double deltaTime)
        {
            img.Transform = Transform;
            text.Transform = Transform;

            if(statusFunc != null)
            {
                status = statusFunc.Invoke();
            }

            img.Color = status ? Theme.StatusOk : Theme.StatusError;
        }
    }
}
