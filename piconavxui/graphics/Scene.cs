using FontStashSharp;
using piconavx.ui.controllers;
using piconavx.ui.graphics.ui;
using Silk.NET.Input;
using Silk.NET.Maths;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace piconavx.ui.graphics
{
    public class Scene
    {
        private static readonly List<Controller> controllers = new List<Controller>();
        private static readonly List<IDisposable> resources = new List<IDisposable>();

        public static void CreateStaticResources()
        {
            Material.DefaultMaterial = AddResource(Material.CreateDefault());
            UIMaterial.DefaultMaterial = AddResource(UIMaterial.CreateDefault());
            Texture.White = AddResource(Texture.CreateWhite(1, 1));
            Texture.Black = AddResource(Texture.CreateBlack(1, 1));
            Texture.UVTest = AddResource(new Texture("assets/textures/uvtest.png"));
        }

        public static void CreateTestScene()
        {
            Model cube = AddController(AddResource(new Model("assets/models/cube.obj")));
            cube.Material = AddResource(new CatMaterial());

            Model lightModel = AddController(AddResource(new Model("assets/models/sphere.obj")));
            lightModel.Transform.Position = new Vector3(1.2f, 1.0f, 2.0f);
            lightModel.Transform.Scale = new Vector3(0.2f);
            lightModel.Material = AddResource(new LightMaterial());

            Light light = AddController(new Light(new Vector3(0.2f), new Vector3(0.5f), lightModel.Transform));

            Camera camera = AddController(new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY));

            OrbitCameraController cameraController = AddController(new OrbitCameraController(camera));

            Canvas canvas = AddController(new Canvas());

            Label label = AddController(new Label("Hello World!!", canvas));
            canvas.AddComponent(label);

            Label label2 = AddController(new Label("This is a label", canvas));
            canvas.AddComponent(label2);
            label2.Bounds = new System.Drawing.RectangleF(0, 70, 20, 10);
            label2.Color = FSColor.OrangeRed;
            label2.ZIndex = 999; // always on top

            Image image = AddController(new Image(canvas));
            canvas.AddComponent(image);
            image.ZIndex = -1; // background
            image.Color = new Rgba32(0, 0, 0, 100);
            image.Bounds = new System.Drawing.RectangleF(0, 0, 800, 0);
            AnchorLayout imageLayout = AddController(new AnchorLayout(image));
            imageLayout.Anchor = Anchor.TopLeft | Anchor.Bottom;
            imageLayout.Insets = new Insets(0);
        }

        public static void CreateUIServer(int port)
        {
            UIServer server = AddController(new UIServer(port));
            server.Start();
        }

        public static void DestroyResources()
        {
            foreach (var resource in resources)
            {
                resource.Dispose();
            }
            resources.Clear();

            foreach (var controller in controllers)
            {
                controller.Unsubscribe();
            }
            controllers.Clear();
        }

        public static T AddResource<T>(T resource) where T : IDisposable
        {
            resources.Add(resource);
            return resource;
        }

        public static T AddController<T>(T controller) where T : Controller
        {
            controllers.Add(controller);
            controller.Subscribe();
            return controller;
        }

        public static T RemoveController<T>(T controller) where T : Controller
        {
            controllers.Remove(controller);
            controller.Unsubscribe();
            return controller;
        }

        public static PrioritizedList<PrioritizedAction<GenericPriority, Rectangle<int>>> ViewportChange = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, float, float>> MouseMove = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, MouseButton>> MouseDown = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, MouseButton>> MouseUp = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, ScrollWheel>> MouseScroll = new();
        public static PrioritizedList<PrioritizedAction<UpdatePriority, double>> Update = new();
        public static PrioritizedList<PrioritizedAction<RenderPriority, double, RenderProperties>> Render = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority>> AppExit = new();

        public static void NotifyViewportChange(Rectangle<int> viewport)
        {
            foreach (var action in ViewportChange)
            {
                action.Action.Invoke(viewport);
            }
        }

        public static void NotifyMouseMove(float dx, float dy)
        {
            foreach (var action in MouseMove)
            {
                action.Action.Invoke(dx, dy);
            }
        }

        public static void NotifyMouseDown(MouseButton button)
        {
            foreach (var action in MouseDown)
            {
                action.Action.Invoke(button);
            }
        }

        public static void NotifyMouseUp(MouseButton button)
        {
            foreach (var action in MouseUp)
            {
                action.Action.Invoke(button);
            }
        }

        public static void NotifyMouseScroll(ScrollWheel scroll)
        {
            foreach (var action in MouseScroll)
            {
                action.Action.Invoke(scroll);
            }
        }

        public static void NotifyUpdate(double deltaTime)
        {
            foreach (var action in Update)
            {
                action.Action.Invoke(deltaTime);
            }
        }

        public static void NotifyRender(double deltaTime, RenderProperties properties)
        {
            foreach (var action in Render)
            {
                action.Action.Invoke(deltaTime, properties);
            }
        }

        public static void NotifyAppExit()
        {
            foreach (var action in AppExit)
            {
                action.Action.Invoke();
            }
        }
    }

    public enum UpdatePriority : int
    {
        BeforeGeneral = 0,
        General = 1,
        AfterGeneral = 2
    }

    public enum RenderPriority : int
    {
        RestoreContext = 0,
        SetupContext = 1,
        DrawObjects = 2,
        PostProcess = 3,
        UI = 4,
        Cleanup = 5
    }
}
