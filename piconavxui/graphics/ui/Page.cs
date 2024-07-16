﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public abstract class Page : UIController
    {
        protected Page(Canvas canvas) : base(canvas)
        {
        }

        public override bool IsRenderable => false;

        public abstract void Show();
        public abstract void Hide();

        // Empty Subscribe/Unsubscribe since Show/Hide are used instead
        public override void Subscribe()
        {
        }
        public override void Unsubscribe()
        {
        }

        protected void SubscribeLater<T>(T controller) where T : Controller
        {
            Scene.InvokeLater(controller.Subscribe, DeferralMode.NextFrame);
        }

        protected void UnsubscribeLater<T>(T controller) where T : Controller
        {
            Scene.InvokeLater(controller.Unsubscribe, DeferralMode.NextFrame);
        }

        protected void SubscribeLater(params Controller[] controllers)
        {
            foreach(var controller in controllers)
            {
                SubscribeLater<Controller>(controller);
            }
        }

        protected void UnsubscribeLater(params Controller[] controllers)
        {
            foreach (var controller in controllers)
            {
                UnsubscribeLater<Controller>(controller);
            }
        }
    }
}
