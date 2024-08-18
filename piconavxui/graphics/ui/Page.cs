using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics.ui
{
    public abstract class Page : Panel
    {
        private Navigator _navigator;
        public Navigator Navigator { get { return _navigator; } }

        protected Page(Canvas canvas, Navigator navigator) : base(canvas)
        {
            _navigator = navigator;
        }

        public abstract void Show();
        public abstract void Hide();

        [DebuggerStepThrough]
        protected T SubscribeLater<T>(T controller) where T : Controller
        {
            Scene.InvokeLater(controller.Subscribe, DeferralMode.NextFrame);
            return controller;
        }

        [DebuggerStepThrough]
        protected T UnsubscribeLater<T>(T controller) where T : Controller
        {
            Scene.InvokeLater(controller.Unsubscribe, DeferralMode.NextFrame);
            return controller;
        }

        [DebuggerStepThrough]
        protected void SubscribeLater(IEnumerable<Controller?> controllers)
        {
            foreach (var controller in controllers)
            {
                if (controller != null)
                    SubscribeLater<Controller>(controller);
            }
        }

        [DebuggerStepThrough]
        protected void UnsubscribeLater(IEnumerable<Controller?> controllers)
        {
            foreach (var controller in controllers)
            {
                if (controller != null)
                    UnsubscribeLater<Controller>(controller);
            }
        }

        [DebuggerStepThrough]
        protected void SubscribeLater(params Controller?[] controllers)
        {
            SubscribeLater(controllers.AsEnumerable());
        }

        [DebuggerStepThrough]
        protected void UnsubscribeLater(params Controller?[] controllers)
        {
            UnsubscribeLater(controllers.AsEnumerable());
        }
    }
}
