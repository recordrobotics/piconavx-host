using piconavx.ui.controllers;
using Silk.NET.Assimp;
using System.Drawing;
using System.Numerics;
using static Silk.NET.Core.Native.WinString;

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
                livePreview.Client = client;
                feedPreview.Client = client;
            }
        }

        private Model reference;
        private Model sensor;
        private LiveClientPreviewController livePreview;
        private Model feedSensor;
        private FeedClientPreviewController feedPreview;
        private Model lightModel;
        private Light light;
        private OrbitCameraController cameraController;

        private FlowPanel controlPanel;
        private AnchorLayout controlPanelLayout;

        private Button startButton;
        private Texture playIcon;
        private Texture stopIcon;

        private Button settingsButton;
        private Texture settingsIcon;

        private Image background;
        private AnchorLayout backgroundAnchor;

        private FlowPanel sidepanel;
        private AnchorLayout sidepanelLayout;

        private FlowPanel headerPanel;
        private Button backButton;
        private Texture backIcon;
        private Label headerTitle;

        private FlowPanel scrollContent;
        private ScrollPanel scrollPanel;
        private FillLayout scrollPanelLayout;
        private AnchorLayout scrollContentLayout;

        private Texture recordIcon;
        private FlowPanel recordingRow;
        private FlowPanel recordPart;
        private Button startRecordingButton;
        private Button stopRecordingButton;

        private ClientDetails dataPanel;

        private SettingsPage settingsPage;

        private UIServer server;

        public ClientPreviewPage(Canvas canvas, Camera camera, Navigator navigator, SettingsPage settingsPage, UIServer server) : base(canvas, navigator)
        {
            this.settingsPage = settingsPage;
            this.server = server;

            reference = Scene.AddResource(new Model("assets/models/reference.obj"));
            reference.RenderPriority = RenderPriority.DrawTransparent;
            reference.SetMaterial("grid", Scene.AddResource(new GridMaterial()));
            reference.SetMaterial("xaxis", Scene.AddResource(new LitMaterial()
            {
                EmissionColor = new Vector3(0.9f, 0.1f, 0.1f),
                DiffuseColor = new Vector3(0, 0, 0),
                SpecularColor = new Vector3(0, 0, 0)
            }));
            reference.SetMaterial("yaxis", Scene.AddResource(new LitMaterial()
            {
                EmissionColor = new Vector3(0.1f, 0.9f, 0.1f),
                DiffuseColor = new Vector3(0, 0, 0),
                SpecularColor = new Vector3(0, 0, 0)
            }));
            reference.SetMaterial("zaxis", Scene.AddResource(new LitMaterial()
            {
                EmissionColor = new Vector3(0.1f, 0.1f, 0.9f),
                DiffuseColor = new Vector3(0, 0, 0),
                SpecularColor = new Vector3(0, 0, 0)
            }));

            sensor = Scene.AddResource(new Model("assets/models/navxmicro.obj"));
            sensor.Material = Scene.AddResource(new SensorMaterial());
            livePreview = new LiveClientPreviewController(sensor.Transform);

            feedSensor = sensor.Clone();
            feedSensor.Material = Scene.AddResource(new SensorFeedMaterial());
            feedPreview = new FeedClientPreviewController(feedSensor);

            lightModel = Scene.AddResource(new Model("assets/models/sphere.obj"));
            lightModel.Transform.Position = new Vector3(1.2f, 1.0f, 2.0f);
            lightModel.Transform.Scale = new Vector3(0.2f);
            lightModel.Material = Scene.AddResource(new LightMaterial());

            light = new Light(new Vector3(0.2f), new Vector3(0.5f), lightModel.Transform);

            cameraController = new OrbitCameraController(camera);
            cameraController.Distance = 6;
            cameraController.Yaw = 45;
            cameraController.Pitch = 25;

            controlPanel = new FlowPanel(canvas);
            controlPanel.Direction = FlowDirection.Horizontal;
            controlPanel.AlignItems = AlignItems.Middle;
            controlPanel.Gap = 27;
            controlPanelLayout = new AnchorLayout(controlPanel, this);
            controlPanelLayout.Anchor = Anchor.Top | Anchor.Right;
            controlPanelLayout.AllowResize = false;
            controlPanelLayout.Insets = new Insets(0, 39, 51, 0);

            playIcon = Scene.AddResource(new Texture("assets/textures/play.png"));
            stopIcon = Scene.AddResource(new Texture("assets/textures/stop.png"));

            startButton = new Button(server.Running ? "Stop" : "Start", canvas);
            startButton.Icon = server.Running ? stopIcon : playIcon;
            startButton.Color = server.Running ? Theme.Error : Theme.Success;
            startButton.IconSize = new SizeF(45, 45);
            startButton.IconGap = server.Running ? 6.0f : 4.5f;
            startButton.FontSize = 15;
            startButton.RenderOffset = new Vector2(0, 2);
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

            sidepanel = new FlowPanel(canvas);
            sidepanel.Direction = FlowDirection.Vertical;
            sidepanel.Gap = 30;
            sidepanel.Padding = new Insets(51);
            sidepanel.Bounds = new RectangleF(0, 0, 831, 0);
            sidepanel.AutoSize = FlowLayout.AutoSize.None;
            sidepanelLayout = new AnchorLayout(sidepanel, this);
            sidepanelLayout.Anchor = Anchor.TopLeft | Anchor.Bottom;
            sidepanelLayout.Insets = new Insets(0);

            background = new Image(canvas);
            background.Color = Theme.Background;
            backgroundAnchor = new AnchorLayout(background, sidepanel);
            backgroundAnchor.Anchor = Anchor.All;
            backgroundAnchor.Insets = new Insets(0);

            headerPanel = new FlowPanel(canvas);
            headerPanel.Direction = FlowDirection.Horizontal;
            headerPanel.AlignItems = AlignItems.Middle;
            headerPanel.Gap = 51;
            sidepanel.Components.Add(headerPanel);

            backIcon = Scene.AddResource(new Texture("assets/textures/back.png"));

            backButton = new Button("Back", canvas);
            backButton.Padding = new Insets(16);
            backButton.IsIconButton = true;
            backButton.Icon = backIcon;
            backButton.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, Navigator.Back);
            headerPanel.Components.Add(backButton);
            backButton.SetTooltip("Go back");

            headerTitle = new Label(() => client?.Id ?? "<UNKNOWN>", canvas);
            headerTitle.FontSize = 20f;
            headerTitle.Font = FontFace.InterSemiBold;
            headerTitle.Color = Theme.Header;
            headerPanel.Components.Add(headerTitle);

            scrollContent = new FlowPanel(canvas);
            scrollPanel = new ScrollPanel(canvas, scrollContent);
            scrollPanel.Horizontal = false;
            sidepanel.Components.Add(scrollPanel);
            scrollPanelLayout = new FillLayout(scrollPanel, sidepanel.VirtualWorkingRectangle);
            scrollPanelLayout.Horizontal = true;
            scrollPanelLayout.Vertical = true;

            scrollContentLayout = new AnchorLayout(scrollContent, scrollPanel);
            scrollContentLayout.Anchor = Anchor.Left | Anchor.Right;
            scrollContentLayout.Insets = new Insets(0);

            recordIcon = Scene.AddResource(new Texture("assets/textures/record.png"));

            

            dataPanel = new ClientDetails(canvas);
            scrollContent.Components.Add(dataPanel);

            UpdateZIndex();
        }

        protected override void UpdateZIndex()
        {
            controlPanel.ZIndex = ZIndex + 1;
            startButton.ZIndex = ZIndex + 1;
            settingsButton.ZIndex = ZIndex + 1;
            background.ZIndex = ZIndex + 2;
            sidepanel.ZIndex = ZIndex + 3;
            headerPanel.ZIndex = ZIndex + 3;
            backButton.ZIndex = ZIndex + 3;
            headerTitle.ZIndex = ZIndex + 3;
            scrollPanel.ZIndex = ZIndex + 3;
            scrollContent.ZIndex = ZIndex + 3;
            dataPanel.ZIndex = ZIndex + 3;
        }

        public override void Show()
        {
            Scene.Update += new PrioritizedAction<UpdatePriority, double>(UpdatePriority.BeforeGeneral, Scene_Update);
            UIServer.ClientDisconnected += new PrioritizedAction<GenericPriority, Client>(GenericPriority.Medium, Server_ClientDisconnected);
            SubscribeLater(
                reference,
                sensor, livePreview,
                feedSensor, feedPreview,
                lightModel, light,
                cameraController,
                controlPanel, controlPanelLayout,
                sidepanel, sidepanelLayout,
                scrollPanelLayout,
                scrollContent, scrollContentLayout,
                background, backgroundAnchor
                );

            Canvas.AddComponent(controlPanel);
            Canvas.AddComponent(startButton);
            Canvas.AddComponent(settingsButton);
            Canvas.AddComponent(sidepanel);
            Canvas.AddComponent(background);
            Canvas.AddComponent(headerPanel);
            Canvas.AddComponent(backButton);
            Canvas.AddComponent(headerTitle);
            Canvas.AddComponent(scrollPanel);
            Canvas.AddComponent(scrollContent);
            Canvas.AddComponent(dataPanel);
        }

        public override void Hide()
        {
            Canvas.RemoveComponent(controlPanel);
            Canvas.RemoveComponent(sidepanel);
            Canvas.RemoveComponent(scrollContent);
            Canvas.RemoveComponent(background);
            UnsubscribeLater(
                reference,
                sensor, livePreview,
                feedSensor, feedPreview,
                lightModel, light,
                cameraController,
                controlPanel, controlPanelLayout,
                sidepanel, sidepanelLayout,
                scrollPanelLayout, 
                scrollContent, scrollContentLayout,
                background, backgroundAnchor
                );
            UIServer.ClientDisconnected -= Server_ClientDisconnected;
            Scene.Update -= Scene_Update;
        }

        private void Scene_Update(double deltaTime)
        {
            startButton.Text = server.Running ? "Stop" : "Start";
            startButton.Color = server.Running ? Theme.Error : Theme.Success;
            startButton.SetTooltip(server.Running ? "Stop host server" : "Run host server");
            startButton.RenderOffset = server.Running ? new System.Numerics.Vector2(0, 0) : new System.Numerics.Vector2(0, 2);
            startButton.Icon = server.Running ? stopIcon : playIcon;
            startButton.IconGap = server.Running ? 6.0f : 4.5f;
        }

        private void Server_ClientDisconnected(Client client)
        {
            if (client == this.client)
            {
                if (Navigator.CurrentPage != this) // navigate to this page
                {
                    Navigator.RemoveIncluding(this);
                }

                Navigator.Back();
                var alert = Alert.CreateOneShot("Connection lost!", $"The connection to '{client.Id}' was interrupted or closed.\nPossible reasons are unreliable wifi, power, or physical connection to the sensor.", Canvas);
                alert.Color = Theme.Error;
                alert.ShowDuration = 10;
            }
        }
    }
}
