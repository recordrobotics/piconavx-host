using FontStashSharp;
using piconavx.ui.controllers;
using piconavx.ui.graphics.ui;
using Silk.NET.Input;
using Silk.NET.Maths;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Concurrent;
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
            LiveClientPreviewController livePreview = AddController(new LiveClientPreviewController(cube.Transform));

            Model lightModel = AddController(AddResource(new Model("assets/models/sphere.obj")));
            lightModel.Transform.Position = new Vector3(1.2f, 1.0f, 2.0f);
            lightModel.Transform.Scale = new Vector3(0.2f);
            lightModel.Material = AddResource(new LightMaterial());

            Light light = AddController(new Light(new Vector3(0.2f), new Vector3(0.5f), lightModel.Transform));

            Camera camera = AddController(new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY));

            OrbitCameraController cameraController = AddController(new OrbitCameraController(camera));

            Canvas canvas = AddController(new Canvas());

            Sidepanel sidepanel = AddController(new Sidepanel("Client Details", canvas));
            canvas.AddComponent(sidepanel);

            ClientDetails dataPanel = AddController(new ClientDetails(canvas));
            canvas.AddComponent(dataPanel);
            AnchorLayout dataPanelLayout = AddController(new AnchorLayout(dataPanel, sidepanel));
            dataPanelLayout.Anchor = Anchor.TopLeft | Anchor.Right;
            dataPanelLayout.Insets = new Insets(50, 130, 50, 0);

            UIServer.ClientConnected += new PrioritizedAction<GenericPriority, Client>(GenericPriority.Medium, (client) =>
            {
                dataPanel.Client = client;
                livePreview.Client = client;
            });

            UIServer.ClientDisconnected += new PrioritizedAction<GenericPriority, Client>(GenericPriority.Medium, (client) =>
            {
                dataPanel.Client = null;
                livePreview.Client = null;
            });
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

        private static ConcurrentQueue<Action> deferredDelegates_nextEvent = new ConcurrentQueue<Action>();
        private static ConcurrentQueue<Action> deferredDelegates_nextFrame = new ConcurrentQueue<Action>();
        private static ConcurrentQueue<Action> deferredDelegates_whenAvailable = new ConcurrentQueue<Action>();

        public static void ExecuteDeferredDelegates(DeferralMode deferralMode)
        {
            switch (deferralMode)
            {
                case DeferralMode.NextEvent:
                    {
                        // Loop to process all delegates for this engine state-related deferral mode
                        while (deferredDelegates_nextEvent.TryDequeue(out var action))
                        {
                            action.Invoke();
                        }
                        break;
                    }
                case DeferralMode.NextFrame:
                    {
                        // Loop to process all delegates for this engine state-related deferral mode
                        while (deferredDelegates_nextFrame.TryDequeue(out var action))
                        {
                            action.Invoke();
                        }
                        break;
                    }
                case DeferralMode.WhenAvailable:
                    {
                        // Only dequeue once because WhenAvailable is not related to engine state and can be done at any time
                        if (deferredDelegates_whenAvailable.TryDequeue(out var action))
                        {
                            action.Invoke();
                        }
                        break;
                    }
            }
        }

        public static void InvokeLater(Action action, DeferralMode deferralMode)
        {
            switch (deferralMode)
            {
                case DeferralMode.NextEvent:
                    deferredDelegates_nextEvent.Enqueue(action);
                    break;
                case DeferralMode.NextFrame:
                    deferredDelegates_nextFrame.Enqueue(action);
                    break;
                case DeferralMode.WhenAvailable:
                    deferredDelegates_whenAvailable.Enqueue(action);
                    break;
            }
        }
    }

    public enum DeferralMode
    {
        /// <summary>
        /// Defers until before the engine dispatches a new event
        /// </summary>
        NextEvent,
        /// <summary>
        /// Defers until before the engine dispatches the update event (invoked before <see cref="DeferralMode.NextEvent"/>)
        /// </summary>
        NextFrame,
        /// <summary>
        /// Defers until before the engine dispatches the update event, split across multiple frames. (invoked after <see cref="DeferralMode.NextEvent"/>)
        /// </summary>
        WhenAvailable
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
