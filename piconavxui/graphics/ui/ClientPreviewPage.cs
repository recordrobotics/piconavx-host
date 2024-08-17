using piconavx.ui.controllers;
using Silk.NET.Assimp;
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

        private Sidepanel sidepanel;
        private ClientDetails dataPanel;
        private AnchorLayout dataPanelLayout;

        public ClientPreviewPage(Canvas canvas, Camera camera, Navigator navigator, UIServer server) : base(canvas, navigator)
        {
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

            sidepanel = new Sidepanel("Client Details", canvas);
            sidepanel.BackClick += new PrioritizedAction<GenericPriority>(GenericPriority.Highest, Navigator.Back);

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
                reference,
                sensor, livePreview,
                feedSensor, feedPreview,
                lightModel, light,
                cameraController,
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
                reference,
                sensor, livePreview,
                feedSensor, feedPreview,
                lightModel, light,
                cameraController,
                sidepanel,
                dataPanel, dataPanelLayout
                );
        }
    }
}
