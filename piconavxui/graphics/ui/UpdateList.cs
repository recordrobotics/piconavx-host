using piconavx.ui.controllers;

namespace piconavx.ui.graphics.ui
{
    public class UpdateList : FlowPanel
    {
        private Dictionary<Client, ClientUpdate?> lastUpdates = [];
        private Client? client;
        public Client? Client { get; set; }

        public UpdateList(Canvas canvas) : base(canvas)
        {
            Direction = FlowDirection.Vertical;
            Gap = 7.5f;
        }
    }
}
