using piconavx.ui.controllers;
using System;
using System.Collections.Generic;
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

        public bool Reversed { get => flowLayout.Reversed; set => flowLayout.Reversed = value; }

        public bool AutoSize { get => flowLayout.AutoSizeContainer; set => flowLayout.AutoSizeContainer = value; }
        public float Gap { get => flowLayout.Gap; set => flowLayout.Gap = value; }

        public FlowPanel(Canvas canvas) : base(canvas)
        {
            flowLayout = new FlowLayout(this);
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
    }
}
