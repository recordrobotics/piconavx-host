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
            Material.CreateStaticResources();
            Material.DefaultMaterial = AddResource(Material.CreateDefault());
            UIMaterial.DefaultMaterial = AddResource(UIMaterial.CreateDefault());
            UIMaterial.ColorMaterial = AddResource(UIMaterial.CreateColorMaterial());
            Texture.White = AddResource(Texture.CreateWhite(1, 1));
            Texture.Black = AddResource(Texture.CreateBlack(1, 1));
            Texture.UVTest = AddResource(new Texture("assets/textures/uvtest.png"));
            Texture.RoundedRect = AddResource(new Texture("assets/textures/roundrect.png")
            {
                Border = new Insets(12),
                WrapMode = TextureWrapMode.Clamp
            });
            Texture.Pill = AddResource(new Texture("assets/textures/pill.png")
            {
                Border = new Insets(32),
                WrapMode = TextureWrapMode.Clamp
            });
        }

        public static void CreateTestScene()
        {
            /*Model reference = AddController(AddResource(new Model("assets/models/reference.obj")));
            reference.RenderPriority = RenderPriority.DrawTransparent;
            reference.SetMaterial("grid", AddResource(new GridMaterial()));
            reference.SetMaterial("xaxis", AddResource(new LitMaterial()
            {
                EmissionColor = new Vector3(0.9f, 0.1f, 0.1f),
                DiffuseColor = new Vector3(0, 0, 0),
                SpecularColor = new Vector3(0, 0, 0)
            }));
            reference.SetMaterial("yaxis", AddResource(new LitMaterial()
            {
                EmissionColor = new Vector3(0.1f, 0.9f, 0.1f),
                DiffuseColor = new Vector3(0, 0, 0),
                SpecularColor = new Vector3(0, 0, 0)
            }));
            reference.SetMaterial("zaxis", AddResource(new LitMaterial()
            {
                EmissionColor = new Vector3(0.1f, 0.1f, 0.9f),
                DiffuseColor = new Vector3(0, 0, 0),
                SpecularColor = new Vector3(0, 0, 0)
            }));

            Model sensor = AddController(AddResource(new Model("assets/models/navxmicro.obj")));
            sensor.Material = AddResource(new SensorMaterial());
            LiveClientPreviewController livePreview = AddController(new LiveClientPreviewController(sensor.Transform));

            Model feedSensor = AddController(sensor.Clone());
            feedSensor.Material = AddResource(new SensorFeedMaterial());
            FeedClientPreviewController feedPreview = AddController(new FeedClientPreviewController(feedSensor));

            Model lightModel = AddController(AddResource(new Model("assets/models/sphere.obj")));
            lightModel.Transform.Position = new Vector3(1.2f, 1.0f, 2.0f);
            lightModel.Transform.Scale = new Vector3(0.2f);
            lightModel.Material = AddResource(new LightMaterial());

            Light light = AddController(new Light(new Vector3(0.2f), new Vector3(0.5f), lightModel.Transform));


            OrbitCameraController cameraController = AddController(new OrbitCameraController(camera));
            cameraController.Distance = 6;
            cameraController.Yaw = 45;
            cameraController.Pitch = 25;*/
            Camera camera = AddController(new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY));

            Canvas canvas = AddController(AddResource(new Canvas()));
            AddController(new InputCanvasDebugController()
            {
/*                ShowBounds = true,
                ShowNonRenderableBounds = true,
                FlowLayoutContentBoundsOutline = true,*/
            });

            ClientPreviewPage clientPreviewPage = AddController(new ClientPreviewPage(canvas));
            clientPreviewPage.ZIndex = 0;
            AnchorLayout clientPreviewPageLayout = AddController(new AnchorLayout(clientPreviewPage));
            clientPreviewPageLayout.Anchor = Anchor.All;
            clientPreviewPageLayout.Insets = new Insets(0);
            //clientPreviewPage.Show();

            ClientListPage clientListPage = AddController(new ClientListPage(canvas));
            clientListPage.ZIndex = 10;
            AnchorLayout clientListPageLayout = AddController(new AnchorLayout(clientListPage));
            clientListPageLayout.Anchor = Anchor.All;
            clientListPageLayout.Insets = new Insets(0);
            clientListPage.Show();
            /*
                        UIServer.ClientConnected += new PrioritizedAction<GenericPriority, Client>(GenericPriority.Medium, (client) =>
                        {
                            clientPreviewPage.Client = client;
                            livePreview.Client = client;
                            feedPreview.Client = client;
                        });

                        UIServer.ClientDisconnected += new PrioritizedAction<GenericPriority, Client>(GenericPriority.Medium, (client) =>
                        {
                            clientPreviewPage.Client = null;
                            feedPreview.Client = null;
                            livePreview.Client = null;
                        });*/
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

        private static bool inEvent = false;
        /// <summary>
        /// Is the engine currently in an event. Use this to make sure not to modify the
        /// subscription list (or call <see cref="Controller.Subscribe"/> / <see cref="Controller.Unsubscribe"/>)
        /// while in an event. Instead, defer the subscription (see <see cref="Scene.InvokeLater(Action, DeferralMode)"/>)
        /// </summary>
        public static bool InEvent { get { return inEvent; } }

        public static PrioritizedList<PrioritizedAction<GenericPriority, Rectangle<int>>> ViewportChange = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, float, float, float, float>> MouseMove = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, MouseButton>> MouseDown = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, MouseButton>> MouseUp = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority, ScrollWheel>> MouseScroll = new();
        public static PrioritizedList<PrioritizedAction<UpdatePriority, double>> Update = new();
        public static PrioritizedList<PrioritizedAction<RenderPriority, double, RenderProperties>> Render = new();
        public static PrioritizedList<PrioritizedAction<GenericPriority>> AppExit = new();

        public static void NotifyViewportChange(Rectangle<int> viewport)
        {
            inEvent = true;
            foreach (var action in ViewportChange)
            {
                action.Action.Invoke(viewport);
            }
            inEvent = false;
        }

        public static void NotifyMouseMove(float x, float y, float dx, float dy)
        {
            inEvent = true;
            foreach (var action in MouseMove)
            {
                action.Action.Invoke(x, y, dx, dy);
            }
            inEvent = false;
        }

        public static void NotifyMouseDown(MouseButton button)
        {
            inEvent = true;
            foreach (var action in MouseDown)
            {
                action.Action.Invoke(button);
            }
            inEvent = false;
        }

        public static void NotifyMouseUp(MouseButton button)
        {
            inEvent = true;
            foreach (var action in MouseUp)
            {
                action.Action.Invoke(button);
            }
            inEvent = false;
        }

        public static void NotifyMouseScroll(ScrollWheel scroll)
        {
            inEvent = true;
            foreach (var action in MouseScroll)
            {
                action.Action.Invoke(scroll);
            }
            inEvent = false;
        }

        public static void NotifyUpdate(double deltaTime)
        {
            inEvent = true;
            foreach (var action in Update)
            {
                action.Action.Invoke(deltaTime);
            }
            inEvent = false;
        }

        public static void NotifyRender(double deltaTime, RenderProperties properties)
        {
            inEvent = true;
            foreach (var action in Render)
            {
                action.Action.Invoke(deltaTime, properties);
            }
            inEvent = false;
        }

        public static void NotifyAppExit()
        {
            inEvent = true;
            foreach (var action in AppExit)
            {
                action.Action.Invoke();
            }
            inEvent = false;
        }

        private static ConcurrentQueue<(Action action, int ttl)> deferredDelegates_nextEvent = new ConcurrentQueue<(Action action, int ttl)>();
        private static ConcurrentQueue<(Action action, int ttl)> deferredDelegates_nextFrame = new ConcurrentQueue<(Action action, int ttl)>();
        private static ConcurrentQueue<Action> deferredDelegates_whenAvailable = new ConcurrentQueue<Action>();

        public static void ExecuteDeferredDelegates(DeferralMode deferralMode)
        {
            switch (deferralMode)
            {
                case DeferralMode.NextEvent:
                    {
                        // Loop to process all delegates for this engine state-related deferral mode
                        List<(Action action, int ttl)> delayed = [];

                        while (deferredDelegates_nextEvent.TryDequeue(out var tuple))
                        {
                            if (tuple.ttl > 0)
                            {
                                delayed.Add((tuple.action, tuple.ttl - 1));
                            }
                            else
                            {
                                tuple.action.Invoke();
                            }
                        }

                        foreach (var tuple in delayed)
                        {
                            deferredDelegates_nextEvent.Enqueue(tuple);
                        }
                        break;
                    }
                case DeferralMode.NextFrame:
                    {
                        // Loop to process all delegates for this engine state-related deferral mode
                        List<(Action action, int ttl)> delayed = [];

                        while (deferredDelegates_nextFrame.TryDequeue(out var tuple))
                        {
                            if (tuple.ttl > 0)
                            {
                                delayed.Add((tuple.action, tuple.ttl - 1));
                            }
                            else
                            {
                                tuple.action.Invoke();
                            }
                        }

                        foreach (var tuple in delayed)
                        {
                            deferredDelegates_nextFrame.Enqueue(tuple);
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
            InvokeLater(action, deferralMode, 0);
        }

        public static void InvokeLater(Action action, DeferralMode deferralMode, int ttl)
        {
            switch (deferralMode)
            {
                case DeferralMode.NextEvent:
                    deferredDelegates_nextEvent.Enqueue((action, ttl));
                    break;
                case DeferralMode.NextFrame:
                    deferredDelegates_nextFrame.Enqueue((action, ttl));
                    break;
                case DeferralMode.WhenAvailable:
                    if (ttl != 0)
                        throw new ArgumentException("Deferring with mode 'WhenAvailable' does not support time to live.", nameof(ttl));
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
        DrawOpaque = 2,
        DrawTransparent = 3,
        PostProcess = 4,
        UI = 5,
        Cleanup = 6
    }
}
