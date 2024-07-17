using piconavx.ui.controllers;

namespace piconavx.ui.graphics.ui
{
    public class ClientPreviewPage : Page
    {
        private Client? client;
        public Client? Client
        {
            get => client; set
            {
                client = value;
                dataPanel.Client = client;
            }
        }

        private Sidepanel sidepanel;
        private ClientDetails dataPanel;
        private AnchorLayout dataPanelLayout;

        public ClientPreviewPage(Canvas canvas) : base(canvas)
        {
            sidepanel = new Sidepanel("Client Details", canvas);

            dataPanel = new ClientDetails(canvas);
            dataPanelLayout = new AnchorLayout(dataPanel, sidepanel);
            dataPanelLayout.Anchor = Anchor.TopLeft | Anchor.Right;
            dataPanelLayout.Insets = new Insets(50, 130, 50, 0);

            UpdateZIndex();
        }

        protected override void UpdateZIndex()
        {
            sidepanel.ZIndex = ZIndex;
            dataPanel.ZIndex = sidepanel.ContentZIndex;
        }

        public override void Show()
        {
            SubscribeLater(
                sidepanel,
                dataPanel, dataPanelLayout
                );

            Canvas.AddComponent(sidepanel);
            Canvas.AddComponent(dataPanel);
        }

        public override void Hide()
        {
            Canvas.RemoveComponent(sidepanel);
            Canvas.RemoveComponent(dataPanel);
            UnsubscribeLater(
                sidepanel,
                dataPanel, dataPanelLayout
                );
        }
    }
}
