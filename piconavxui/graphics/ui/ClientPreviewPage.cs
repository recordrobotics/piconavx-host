using piconavx.ui.controllers;
using System.Drawing;
using System.Numerics;

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
        private FillLayout headerPanelLayout;
        private Image headerBackground;
        private AnchorLayout headerBackgroundAnchor;
        private Button backButton;
        private Texture backIcon;
        private Label headerTitle;

        private FlowPanel scrollContent;
        private ScrollPanel scrollPanel;
        private FillLayout scrollPanelLayout;
        private AnchorLayout scrollContentLayout;

        private Texture recordIcon;
        private Texture manageRecordingsIcon;
        private FlowPanel recordingRow;
        private AnchorLayout recordingRowLayout;
        private FlowPanel recordPart;
        private Button startRecordingButton;
        private Button stopRecordingButton;
        private Button manageRecordingsButton;

        private Image topBackground;
        private AnchorLayout topBackgroundAnchor;

        private Image bottomBackground;
        private AnchorLayout bottomBackgroundAnchor;

        private ClientDetails dataPanel;
        private AnchorLayout dataPanelLayout;

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

            topBackground = new Image(canvas);
            topBackground.Color = Theme.Background;
            topBackground.Bounds = new RectangleF(0, 0, 0, 51);
            topBackgroundAnchor = new AnchorLayout(topBackground, sidepanel);
            topBackgroundAnchor.Anchor = Anchor.Left | Anchor.Top | Anchor.Right;
            topBackgroundAnchor.Insets = new Insets(0);

            bottomBackground = new Image(canvas);
            bottomBackground.Color = Theme.Background;
            bottomBackground.Bounds = new RectangleF(0, 0, 0, 51);
            bottomBackgroundAnchor = new AnchorLayout(bottomBackground, sidepanel);
            bottomBackgroundAnchor.Anchor = Anchor.Left | Anchor.Bottom | Anchor.Right;
            bottomBackgroundAnchor.Insets = new Insets(0);

            headerPanel = new FlowPanel(canvas);
            headerPanel.Direction = FlowDirection.Horizontal;
            headerPanel.AlignItems = AlignItems.Middle;
            headerPanel.AutoSize = FlowLayout.AutoSize.Y;
            headerPanel.Gap = 51;
            headerPanel.Padding = new Insets(0, 0, 0, 30);
            sidepanel.Components.Add(headerPanel);
            headerPanelLayout = new FillLayout(headerPanel, sidepanel.VirtualWorkingRectangle);
            headerPanelLayout.Horizontal = true;

            headerBackground = new Image(canvas);
            headerBackground.Color = Theme.Background;
            headerBackgroundAnchor = new AnchorLayout(headerBackground, headerPanel);
            headerBackgroundAnchor.Anchor = Anchor.All;
            headerBackgroundAnchor.Insets = new Insets(0);

            backIcon = Scene.AddResource(new Texture("assets/textures/back.png"));

            backButton = new Button("Back", canvas);
            backButton.Padding = new Insets(16);
            backButton.IsIconButton = true;
            backButton.Icon = backIcon;
            backButton.Click += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, Navigator.Back);
            headerPanel.Components.Add(backButton);
            backButton.SetTooltip("Go back");

            headerTitle = new Label(() => (client?.Id ?? "<UNKNOWN>", null), canvas);
            headerTitle.FontSize = 20f;
            headerTitle.Font = FontFace.InterSemiBold;
            headerTitle.Color = Theme.Header;
            headerPanel.Components.Add(headerTitle);

            scrollContent = new FlowPanel(canvas);
            scrollContent.Direction = FlowDirection.Vertical;
            scrollContent.Gap = 15;

            scrollPanel = new ScrollPanel(canvas, scrollContent);
            scrollPanel.Horizontal = false;
            sidepanel.Components.Add(scrollPanel);
            scrollPanelLayout = new FillLayout(scrollPanel, sidepanel.VirtualWorkingRectangle);
            scrollPanelLayout.Horizontal = true;
            scrollPanelLayout.Vertical = true;

            scrollContentLayout = new AnchorLayout(scrollContent, scrollPanel.VirtualWorkingRectangle);
            scrollContentLayout.Anchor = Anchor.Left | Anchor.Right;
            scrollContentLayout.Insets = new Insets(0);

            recordIcon = Scene.AddResource(new Texture("assets/textures/record.png"));
            manageRecordingsIcon = Scene.AddResource(new Texture("assets/textures/recordings.png"));

            recordingRow = new FlowPanel(canvas);
            recordingRow.Direction = FlowDirection.Horizontal;
            recordingRow.Stretch = true;
            recordingRow.AutoSize = FlowLayout.AutoSize.Y;
            recordingRow.AlignItems = AlignItems.Middle;
            recordingRow.Padding = new Insets(0, 0, 0, 6);
            scrollContent.Components.Add(recordingRow);
            recordingRowLayout = new AnchorLayout(recordingRow, scrollContent.VirtualWorkingRectangle);
            recordingRowLayout.Anchor = Anchor.Left | Anchor.Right;
            recordingRowLayout.Insets = new Insets(0);

            recordPart = new FlowPanel(canvas);
            recordPart.Direction = FlowDirection.Horizontal;
            recordPart.AlignItems = AlignItems.Middle;
            recordPart.Gap = 15;
            recordingRow.Components.Add(recordPart);

            startRecordingButton = new Button("Start Recording", canvas);
            startRecordingButton.Color = Theme.Primary;
            startRecordingButton.FontSize = 13;
            startRecordingButton.IconSize = new SizeF(30, 30);
            startRecordingButton.IconGap = 15;
            startRecordingButton.CornerSize = new Size(9, 9);
            startRecordingButton.AutoSize = Button.AutoSizeMode.TextAndIcon;
            startRecordingButton.Padding = new Insets(15, 9, 15, 9);
            startRecordingButton.Icon = recordIcon;
            startRecordingButton.SetTooltip("Begin feed recording");
            recordPart.Components.Add(startRecordingButton);

            stopRecordingButton = new Button("Stop Recording", canvas);
            stopRecordingButton.Color = Theme.Error;
            stopRecordingButton.IsIconButton = true;
            stopRecordingButton.IconSize = new SizeF(33, 33);
            stopRecordingButton.CornerSize = new Size(9, 9);
            stopRecordingButton.Padding = new Insets(7.5f);
            stopRecordingButton.Icon = stopIcon;
            stopRecordingButton.SetTooltip("Stop recording");
            recordPart.Components.Add(stopRecordingButton);

            manageRecordingsButton = new Button("Manage Recordings", canvas);
            manageRecordingsButton.Color = Theme.Neutral;
            manageRecordingsButton.FontSize = 13;
            manageRecordingsButton.IconSize = new SizeF(30, 30);
            manageRecordingsButton.IconGap = 15;
            manageRecordingsButton.CornerSize = new Size(9, 9);
            manageRecordingsButton.AutoSize = Button.AutoSizeMode.TextAndIcon;
            manageRecordingsButton.Padding = new Insets(15, 9, 15, 9);
            manageRecordingsButton.Icon = manageRecordingsIcon;
            manageRecordingsButton.SetTooltip("View and manage recordings");
            recordingRow.Components.Add(manageRecordingsButton);

            dataPanel = new ClientDetails(canvas);
            dataPanelLayout = new AnchorLayout(dataPanel, scrollContent);
            dataPanelLayout.Anchor = Anchor.Left | Anchor.Right;
            dataPanelLayout.Insets = new Insets(0);
            scrollContent.Components.Add(dataPanel);

            UpdateZIndex();
        }

        protected override void UpdateZIndex()
        {
            controlPanel.ZIndex = ZIndex + 1;
            startButton.ZIndex = ZIndex + 1;
            settingsButton.ZIndex = ZIndex + 1;
            background.ZIndex = ZIndex + 2;
            topBackground.ZIndex = ZIndex + 10;
            bottomBackground.ZIndex = ZIndex + 10;
            sidepanel.ZIndex = ZIndex + 3;
            headerPanel.ZIndex = ZIndex + 11;
            headerBackground.ZIndex = ZIndex + 10;
            backButton.ZIndex = ZIndex + 11;
            headerTitle.ZIndex = ZIndex + 11;
            scrollPanel.ZIndex = ZIndex + 3;
            scrollContent.ZIndex = ZIndex + 4;
            recordingRow.ZIndex = ZIndex + 4;
            recordPart.ZIndex = ZIndex + 4;
            startRecordingButton.ZIndex = ZIndex + 4;
            stopRecordingButton.ZIndex = ZIndex + 4;
            manageRecordingsButton.ZIndex = ZIndex + 4;
            dataPanel.ZIndex = ZIndex + 4;
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
                headerPanelLayout,
                scrollPanelLayout,
                scrollContent, scrollContentLayout,
                background, backgroundAnchor,
                headerBackground, headerBackgroundAnchor,
                topBackground, topBackgroundAnchor,
                bottomBackground, bottomBackgroundAnchor,
                recordingRowLayout,
                dataPanelLayout
                );

            Canvas.AddComponent(controlPanel);
            Canvas.AddComponent(startButton);
            Canvas.AddComponent(settingsButton);
            Canvas.AddComponent(sidepanel);
            Canvas.AddComponent(background);
            Canvas.AddComponent(topBackground);
            Canvas.AddComponent(bottomBackground);
            Canvas.AddComponent(headerPanel);
            Canvas.AddComponent(headerBackground);
            Canvas.AddComponent(backButton);
            Canvas.AddComponent(headerTitle);
            Canvas.AddComponent(scrollPanel);
            Canvas.AddComponent(scrollContent);
            Canvas.AddComponent(recordingRow);
            Canvas.AddComponent(recordPart);
            Canvas.AddComponent(startRecordingButton);
            Canvas.AddComponent(stopRecordingButton);
            Canvas.AddComponent(manageRecordingsButton);
            Canvas.AddComponent(dataPanel);
        }

        public override void Hide()
        {
            Canvas.RemoveComponent(controlPanel);
            Canvas.RemoveComponent(sidepanel);
            Canvas.RemoveComponent(scrollContent);
            Canvas.RemoveComponent(background);
            Canvas.RemoveComponent(topBackground);
            Canvas.RemoveComponent(bottomBackground);
            Canvas.RemoveComponent(headerBackground);
            UnsubscribeLater(
                reference,
                sensor, livePreview,
                feedSensor, feedPreview,
                lightModel, light,
                cameraController,
                controlPanel, controlPanelLayout,
                sidepanel, sidepanelLayout,
                headerPanelLayout,
                scrollPanelLayout, 
                scrollContent, scrollContentLayout,
                background, backgroundAnchor,
                topBackground, topBackgroundAnchor,
                bottomBackground, bottomBackgroundAnchor,
                headerBackground, headerBackgroundAnchor,
                recordingRowLayout,
                dataPanelLayout
                );
            UIServer.ClientDisconnected -= Server_ClientDisconnected;
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
