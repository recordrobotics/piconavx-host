using FontStashSharp;
using piconavx.ui.graphics.font;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;

namespace piconavx.ui.graphics
{
    public class Window
    {
        private IWindow window;
        private IKeyboard? primaryKeyboard;
        private IInputContext? input;
        private static GL? gl;
        private static Dictionary<FontFace, FontSystem> fontSystems = new Dictionary<FontFace, FontSystem>();
        private static Renderer? fontRenderer;
        private Vector2 prevMousePos;

        public static GL GL { get => gl!; }
        public static IReadOnlyDictionary<FontFace, FontSystem> FontSystems { get => fontSystems.AsReadOnly(); }
        public static Renderer FontRenderer { get => fontRenderer!; }

        private static Window? currentWindow;
        public static Window Current { get => currentWindow!; }

        public IKeyboard? PrimaryKeyboard { get => primaryKeyboard; }
        public IInputContext? Input { get => input; }
        public IWindow Internal { get=> window; }

        public event Action? Load;

        public Window(WindowOptions options)
        {
            window = Silk.NET.Windowing.Window.Create(options);
            window.Load += Window_Load;
            window.Closing += Window_Closing;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.FramebufferResize += Window_FramebufferResize;

            currentWindow = this;
        }

        private void Window_Update(double deltaTime)
        {
            currentWindow = this;
            Scene.ExecuteDeferredDelegates(DeferralMode.NextFrame);
            Scene.ExecuteDeferredDelegates(DeferralMode.NextEvent);
            Scene.ExecuteDeferredDelegates(DeferralMode.WhenAvailable);
            Scene.NotifyUpdate(deltaTime);
        }

        private void Window_Render(double deltaTime)
        {
            currentWindow = this;
            Scene.ExecuteDeferredDelegates(DeferralMode.NextEvent);
            RenderProperties properties = new RenderProperties();
            Scene.NotifyRender(deltaTime, properties);
        }

        private void Window_FramebufferResize(Silk.NET.Maths.Vector2D<int> newSize)
        {
            currentWindow = this;
            Scene.ExecuteDeferredDelegates(DeferralMode.NextEvent);
            Scene.NotifyViewportChange(new Silk.NET.Maths.Rectangle<int>(0, 0, newSize));
        }

        private void AddFont(FontFace font, string path)
        {
            var fontSystem = new FontSystem(new FontSystemSettings
            {
                FontResolutionFactor = 2,
                KernelWidth = 1,
                KernelHeight = 1
            });
            fontSystem.AddFont(EmbeddedResource.ReadAllBytes(path));
            fontSystems.Add(font, fontSystem);
        }

        private void Window_Load()
        {
            window.Center();

            currentWindow = this;
            gl = GL.GetApi(window);

            fontRenderer = new Renderer();

            AddFont(FontFace.InterRegular, "assets/fonts/Inter-Regular.ttf");
            AddFont(FontFace.InterLight, "assets/fonts/Inter-Light.ttf");
            AddFont(FontFace.InterSemiBold, "assets/fonts/Inter-SemiBold.ttf");
            AddFont(FontFace.InterBold, "assets/fonts/Inter-Bold.ttf");

            Scene.CreateStaticResources();

            input = window.CreateInput();
            primaryKeyboard = input.Keyboards.Count > 0 ? input.Keyboards[0] : null;

            for (int i = 0; i < input.Mice.Count; i++)
            {
                input.Mice[i].Cursor.CursorMode = CursorMode.Normal;
                input.Mice[i].MouseMove += Window_MouseMove;
                input.Mice[i].Scroll += Window_Scroll;
                input.Mice[i].MouseDown += Window_MouseDown;
                input.Mice[i].MouseUp += Window_MouseUp;
            }

            Load?.Invoke();
        }

        private void Window_MouseMove(IMouse mouse, Vector2 position)
        {
            currentWindow = this;
            if (prevMousePos == default) { prevMousePos = position; }
            else
            {
                float dx = (position.X - prevMousePos.X);
                float dy = (position.Y - prevMousePos.Y);
                prevMousePos = position;

                Scene.ExecuteDeferredDelegates(DeferralMode.NextEvent);
                Scene.NotifyMouseMove(position.X, position.Y, dx, dy);
            }
        }

        private void Window_MouseDown(IMouse mouse, MouseButton button)
        {
            currentWindow = this;
            Scene.ExecuteDeferredDelegates(DeferralMode.NextEvent);
            Scene.NotifyMouseDown(button);
        }

        private void Window_MouseUp(IMouse mouse, MouseButton button)
        {
            currentWindow = this;
            Scene.ExecuteDeferredDelegates(DeferralMode.NextEvent);
            Scene.NotifyMouseUp(button);
        }

        private void Window_Scroll(IMouse mouse, ScrollWheel scroll)
        {
            currentWindow = this;
            Scene.ExecuteDeferredDelegates(DeferralMode.NextEvent);
            Scene.NotifyMouseScroll(scroll);
        }

        public void Run()
        {
            window.Run();
            window.Dispose();
        }

        private void Window_Closing()
        {
            currentWindow = this;
            Scene.NotifyAppExit();
            Scene.DestroyResources();
            fontRenderer?.Dispose();
        }
    }

    public enum FontFace
    {
        InterRegular,
        InterLight,
        InterSemiBold,
        InterBold
    }
}
