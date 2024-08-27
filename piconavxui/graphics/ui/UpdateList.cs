using piconavx.ui.controllers;

namespace piconavx.ui.graphics.ui
{
    public class UpdateList : FlowPanel
    {
        public UpdateList(Canvas canvas) : base(canvas)
        {
            Direction = FlowDirection.Vertical;
            Gap = 7.5f;
        }
    }
}
