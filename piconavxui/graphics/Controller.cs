using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace piconavx.ui.graphics
{
    public abstract class Controller
    {
        /// <summary>
        /// Subscribes this controller to engine events
        /// </summary>
        /// <remarks>
        /// Note: This cannot be called from inside an engine event, since that would modify the subscription list while enumerating.
        /// To create controllers during engine events, use <see cref="Scene.InvokeLater(Action, DeferralMode)"/> to defer the subscription until the engine is available.
        /// </remarks>
        public abstract void Subscribe();

        /// <summary>
        /// Unsubscribes this controller from engine events
        /// </summary>
        /// <remarks>
        /// Note: This cannot be called from inside an engine event, since that would modify the subscription list while enumerating.
        /// To destroy controllers during engine events, use <see cref="Scene.InvokeLater(Action, DeferralMode)"/> to defer the subscription until the engine is available.
        /// </remarks>
        public abstract void Unsubscribe();
    }
}
