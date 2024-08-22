using piconavx.ui.controllers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public class FlowPanel : Panel
    {
        private FlowLayout flowLayout;

        public List<UIController> Components { get => flowLayout.Components; }

        public FlowDirection Direction { get => flowLayout.Direction; set => flowLayout.Direction = value; }
        public AlignItems AlignItems { get => flowLayout.AlignItems; set => flowLayout.AlignItems = value; }
        public AlignItems JustifyContent { get => flowLayout.JustifyContent; set => flowLayout.JustifyContent = value; }

        public bool Reversed { get => flowLayout.Reversed; set => flowLayout.Reversed = value; }
        public bool Wrap { get => flowLayout.Wrap; set => flowLayout.Wrap = value; }
        public bool Stretch { get => flowLayout.Stretch; set => flowLayout.Stretch = value; }

        public FlowLayout.AutoSize AutoSize { get => flowLayout.AutoSizeContainer; set => flowLayout.AutoSizeContainer = value; }
        public float Gap { get => flowLayout.Gap; set => flowLayout.Gap = value; }
        public Insets Padding { get => flowLayout.Padding; set => flowLayout.Padding = value; }

        public RectangleF ContentBounds => flowLayout.ContentBounds;
        public RectangleF WorkingRectangle => flowLayout.WorkingRectangle;
        private VirtualUIController virtualWorkingRectangle;
        public VirtualUIController VirtualWorkingRectangle => virtualWorkingRectangle;

        public FlowPanel(Canvas canvas) : base(canvas)
        {
            flowLayout = new FlowLayout(this);
            virtualWorkingRectangle = new VirtualUIController(canvas);
            virtualWorkingRectangle.GetBounds = () => flowLayout.WorkingRectangle;
        }

        public override void Subscribe()
        {
            base.Subscribe();
            foreach (var component in flowLayout.Components)
            {
                component.Subscribe();
            }
            flowLayout.Subscribe();
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            foreach (var component in flowLayout.Components)
            {
                component.Unsubscribe();
            }
            flowLayout.Unsubscribe();
        }

        public override void OnAdd()
        {
            base.OnAdd();
            flowLayout.Visible = true;
        }

        public override void OnRemove()
        {
            base.OnRemove();
            flowLayout.Visible = false;
            foreach (var component in Components)
            {
                Scene.InvokeLater(component.Unsubscribe, DeferralMode.NextEvent); // Destroy as soon as possible
                Canvas.RemoveComponent(component);
            }
        }
    }
}
