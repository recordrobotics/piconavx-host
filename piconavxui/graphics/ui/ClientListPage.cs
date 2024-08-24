using piconavx.ui.controllers;
using System.Collections.Frozen;
using System.Drawing;
using System.Net;

namespace piconavx.ui.graphics.ui
{
    public class ClientListPage : Page
    {
        private static Texture? cardShadowTexture;

        private Image background;
        private AnchorLayout backgroundAnchor;

        private Panel headerPanel;
        private AnchorLayout headerPanelLayout;

        private Image headerBackground;
        private AnchorLayout headerBackgroundLayout;

        private Label header;
        private AnchorLayout headerLayout;

        private FlowPanel controlPanel;
        private AnchorLayout controlPanelLayout;

        private Button startButton;
        private Texture playIcon;
        private Texture stopIcon;

        private Button settingsButton;
        private Texture settingsIcon;

        private ScrollPanel clientListContainer;
        private AnchorLayout clientListContainerLayout;

        private FlowPanel clientList;
        private AnchorLayout clientListLayout;

        private Image bottomBar;
        private AnchorLayout bottomBarLayout;

        private Label statusLabel;
        private AnchorLayout statusLabelLayout;

        private ClientPreviewPage clientPreviewPage;
        private SettingsPage settingsPage;

        private UIServer server;

        private List<ClientCardUpdateController> cardControllers = [];

        public ClientListPage(Canvas canvas, Navigator navigator, ClientPreviewPage clientPreviewPage, SettingsPage settingsPage, UIServer server) : base(canvas, navigator)
        {
            this.clientPreviewPage = clientPreviewPage;
            this.settingsPage = settingsPage;
            this.server = server;

            cardShadowTexture ??= Scene.AddResource(new Texture("assets/textures/cardshadow.png")
            {
                Border = new Insets(32),
                WrapMode = TextureWrapMode.Clamp
            });

            background = new Image(canvas);
            background.Color = Theme.Background;
            backgroundAnchor = new AnchorLayout(background, this);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            headerPanel = new Panel(canvas);
            headerPanel.Bounds = new RectangleF(0, 0, 0, 140);
            headerPanelLayout = new AnchorLayout(headerPanel, this);
            headerPanelLayout.Anchor = Anchor.TopLeft | Anchor.Right;
            headerPanelLayout.Insets = new Insets(0);

            headerBackground = new Image(canvas);
            headerBackground.Color = Theme.Background;
            headerBackgroundLayout = new AnchorLayout(headerBackground, headerPanel);
            headerBackgroundLayout.Anchor = Anchor.All;
            headerBackgroundLayout.Insets = new Insets(0);

            header = new Label("Connected Clients", canvas);
            header.FontSize = 24.5f;
            header.Font = FontFace.InterSemiBold;
            header.Color = Theme.Header;
            headerLayout = new AnchorLayout(header, headerPanel);
            headerLayout.Anchor = Anchor.TopLeft | Anchor.Bottom;
            headerLayout.AllowResize = false;
            headerLayout.Insets = new Insets(53, 0, 0, 0);

            controlPanel = new FlowPanel(canvas);
            controlPanel.Direction = FlowDirection.Horizontal;
            controlPanel.AlignItems = AlignItems.Middle;
            controlPanel.Gap = 27;
            controlPanelLayout = new AnchorLayout(controlPanel, headerPanel);
            controlPanelLayout.Anchor = Anchor.Top | Anchor.Right | Anchor.Bottom;
            controlPanelLayout.AllowResize = false;
            controlPanelLayout.Insets = new Insets(0, 0, 51, 0);

            playIcon = Scene.AddResource(new Texture("assets/textures/play.png"));
            stopIcon = Scene.AddResource(new Texture("assets/textures/stop.png"));

            startButton = new Button(server.Running ? "Stop" : "Start", canvas);
            startButton.Icon = server.Running ? stopIcon : playIcon;
            startButton.Color = server.Running ? Theme.Error : Theme.Success;
            startButton.IconSize = new SizeF(45, 45);
            startButton.IconGap = server.Running ? 6.0f : 4.5f;
            startButton.FontSize = 15;
            startButton.AutoSize = Button.AutoSizeMode.TextAndIcon;
            startButton.Padding = new Insets(11.25f, 7.5f, 20.25f, 7.5f);
            startButton.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, () =>
            {
                if (server.Running)
                    server.Stop();
                else
                    server.Start(canvas);
            });
            controlPanel.Components.Add(startButton);
            startButton.SetTooltip(server.Running ? "Stop host server" : "Run host server");

            settingsIcon = Scene.AddResource(new Texture("assets/textures/settings.png"));

            settingsButton = new Button("Settings", canvas);
            settingsButton.Padding = new Insets(16);
            settingsButton.IsIconButton = true;
            settingsButton.Icon = settingsIcon;
            settingsButton.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, () => Navigator.Push(settingsPage));
            controlPanel.Components.Add(settingsButton);
            settingsButton.SetTooltip("Open settings");

            clientList = new FlowPanel(canvas);
            clientList.AutoSize = FlowLayout.AutoSize.Y;
            clientList.Direction = FlowDirection.Horizontal;
            clientList.JustifyContent = AlignItems.Middle;
            clientList.Padding = new Insets(0, 10, 0, 0);
            clientList.Gap = 120;
            clientList.Wrap = true;

            clientListContainer = new ScrollPanel(canvas, clientList);
            clientListContainerLayout = new AnchorLayout(clientListContainer, this);
            clientListContainerLayout.Anchor = Anchor.All;
            clientListContainerLayout.Insets = new Insets(52.5f, 140, 52.5f, 60f);

            clientListLayout = new AnchorLayout(clientList, clientListContainer.VirtualWorkingRectangle);
            clientListLayout.Anchor = Anchor.Left | Anchor.Right;
            clientListLayout.Insets = new Insets(0);

            bottomBar = new Image(canvas);
            bottomBar.Color = Theme.Background;
            bottomBar.Bounds = new RectangleF(0, 0, 0, 60);
            bottomBarLayout = new AnchorLayout(bottomBar, this);
            bottomBarLayout.Anchor = Anchor.Left | Anchor.Right | Anchor.Bottom;
            bottomBarLayout.Insets = new Insets(0);

            statusLabel = new Label(() =>
            {
                if (!server.Running)
                    return "Server not started";

                if (server.LocalEndpoint is IPEndPoint endpoint)
                {
                    if (endpoint.Address.Equals(IPAddress.Any))
                    {
                        var addresses = server.GetInterfaceAddresses();
                        if (addresses == null)
                            return $"Server running on {endpoint.Address}:{endpoint.Port}";

                        return $"Server running on {string.Join(", ", addresses.Select(v => $"{v}:{endpoint.Port}"))}";
                    }
                    else
                    {
                        return $"Server running on {endpoint.Address}:{endpoint.Port}";
                    }
                }
                else
                {
                    return "Server running with unsupported endpoint";
                }
            }, canvas);
            statusLabel.FontSize = 13;
            statusLabel.Font = FontFace.InterLight;
            statusLabel.Color = Theme.TextSecondary;
            statusLabelLayout = new AnchorLayout(statusLabel, this);
            statusLabelLayout.Anchor = Anchor.Left | Anchor.Right | Anchor.Bottom;
            statusLabelLayout.AllowResize = false;
            statusLabelLayout.Insets = new Insets(0, 0, 0, 16.5f);

            ClientCardUpdateController.RemoveCard = RemoveCardEvt;

            UpdateZIndex();
        }

        protected override void UpdateZIndex()
        {
            background.ZIndex = ZIndex;
            headerBackground.ZIndex = ZIndex + 8;
            header.ZIndex = ZIndex + 9;
            controlPanel.ZIndex = ZIndex + 9;
            startButton.ZIndex = ZIndex + 9;
            settingsButton.ZIndex = ZIndex + 9;
            bottomBar.ZIndex = ZIndex + 6;
            statusLabel.ZIndex = ZIndex + 7;
            clientListContainer.ZIndex = ZIndex + 1;

            foreach (var component in clientList.Components)
            {
                component.ZIndex = ZIndex + 1;
            }
        }

        private void AddCardEvt(Client client)
        {
            Scene.InvokeLater(() => AddCard(client, true), DeferralMode.NextFrame);
        }

        private void RemoveCardEvt(Client client)
        {
            Scene.InvokeLater(() =>
            {
                ClientCardUpdateController? controller = cardControllers.Find(c => c.Client == client);
                if (controller != null)
                {
                    controller.Target.Unsubscribe();
                    clientList.Components.Remove(controller.Target);
                    Canvas.RemoveComponent(controller.Target);
                    UpdateZIndex();

                    cardControllers.Remove(controller);
                    controller.Unsubscribe();
                }
            }, DeferralMode.NextFrame);
        }

        private void AddCard(Client client, bool revalidate)
        {
            var component = new ClientCard(Canvas);
            if(revalidate)
                component.Subscribe();

            ClientCardUpdateController.UpdateClient(component, client, null);
            component.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, new Func<Client, Action>((client) => new Action(() => OnCardClick(client)))(client));

            var controller = new ClientCardUpdateController(component)
            {
                Client = client
            };
            
            if(revalidate)
                controller.Subscribe();

            cardControllers.Add(controller);
            clientList.Components.Add(component);
            Canvas.AddComponent(component);

            if (revalidate)
                UpdateZIndex();

            if(revalidate)
                Scene.ComplainLayoutShift(2);
        }

        public override void Show()
        {
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
            UIServer.ClientConnected += new PrioritizedAction<GenericPriority, Client>(GenericPriority.Medium, AddCardEvt);
            UIServer.ClientDisconnected += new PrioritizedAction<GenericPriority, Client>(GenericPriority.Medium, RemoveCardEvt);

            SubscribeLater(
                background, backgroundAnchor,
                headerPanel, headerPanelLayout,
                header, headerLayout,
                headerBackground, headerBackgroundLayout,
                controlPanel, controlPanelLayout,
                clientListContainer, clientListContainerLayout,
                clientList, clientListLayout,
                bottomBar, bottomBarLayout,
                statusLabel, statusLabelLayout
                );

            Canvas.AddComponent(background);
            Canvas.AddComponent(headerPanel);
            Canvas.AddComponent(headerBackground);
            Canvas.AddComponent(header);
            Canvas.AddComponent(controlPanel);
            Canvas.AddComponent(startButton);
            Canvas.AddComponent(settingsButton);
            Canvas.AddComponent(clientListContainer);
            Canvas.AddComponent(clientList);
            Canvas.AddComponent(bottomBar);
            Canvas.AddComponent(statusLabel);

            cardControllers.Clear();

            clientList.Components.Clear();

            var clients = server.Clients.ToArray();
            foreach (var client in clients)
            {
                AddCard(client, false);
            }

            Scene.InvokeLater(() => Scene.ComplainLayoutShift(2), DeferralMode.NextEvent);

            SubscribeLater(cardControllers);

            UpdateZIndex();
        }

        private void OnCardClick(Client client)
        {
            clientPreviewPage.Client = client;
            Navigator.Push(clientPreviewPage);
        }

        public override void Hide()
        {
            Canvas.RemoveComponent(background);
            Canvas.RemoveComponent(headerPanel);
            Canvas.RemoveComponent(headerBackground);
            Canvas.RemoveComponent(header);
            Canvas.RemoveComponent(controlPanel);
            Canvas.RemoveComponent(clientList);
            Canvas.RemoveComponent(clientListContainer);
            Canvas.RemoveComponent(bottomBar);
            Canvas.RemoveComponent(statusLabel);
            UnsubscribeLater(
                background, backgroundAnchor,
                headerPanel, headerPanelLayout,
                headerBackground, headerBackgroundLayout,
                header, headerLayout,
                controlPanel, controlPanelLayout,
                clientListContainer, clientListContainerLayout,
                clientList, clientListLayout,
                bottomBar, bottomBarLayout,
                statusLabel, statusLabelLayout
                );

            UnsubscribeLater(cardControllers);

            UIServer.ClientConnected -= AddCardEvt;
            UIServer.ClientDisconnected -= AddCardEvt;
            Scene.Update -= Scene_Update;
        }

        private void Scene_Update(double deltaTime)
        {
            startButton.Text = server.Running ? "Stop" : "Start";
            startButton.Color = server.Running ? Theme.Error : Theme.Success;
            startButton.SetTooltip(server.Running ? "Stop host server" : "Run host server");
            startButton.Icon = server.Running ? stopIcon : playIcon;
            startButton.IconGap = server.Running ? 6.0f : 4.5f;
        }
    }
}
