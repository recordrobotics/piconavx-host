using FontStashSharp;
using piconavx.ui.graphics.font;
using piconavx.ui.graphics.ui;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;

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

        public Vector2D<int> FramebufferSize { get => new((int)(window.FramebufferSize.X / UIController.GlobalScale.X), (int)(window.FramebufferSize.Y / UIController.GlobalScale.Y)); }

        public event Action? Load;

        public Window(WindowOptions options)
        {
            window = Silk.NET.Windowing.Window.Create(options);
            window.Load += Window_Load;
            window.Closing += Window_Closing;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.FramebufferResize += Window_FramebufferResize;
            window.Move += Window_Move;

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

        public static Vector2 GetSystemDpiScale()
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(8, 1))
            {
                var monitor = PInvoke.MonitorFromWindow(HWND.Null, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);
                PInvoke.GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
                float dpiScaleX = dpiX / 96.0f;
                float dpiScaleY = dpiY / 96.0f;
                return new Vector2(dpiScaleX / 1.5f, dpiScaleY / 1.5f);
            }

            return Vector2.One;
        }

        public Vector2 GetDpiScale()
        {
            if (window.Native?.Win32.HasValue ?? false)
            {
                var hwnd = new HWND(window.Native.Win32.Value.Hwnd);

                if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 14393))
                {
                    uint dpi = PInvoke.GetDpiForWindow(hwnd);
                    float dpiScale = dpi / 96.0f;
                    return new Vector2(dpiScale / 1.5f);
                }
                else if (OperatingSystem.IsWindowsVersionAtLeast(8, 1))
                {
                    var monitor = PInvoke.MonitorFromWindow(hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
                    PInvoke.GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY);
                    float dpiScaleX = dpiX / 96.0f;
                    float dpiScaleY = dpiY / 96.0f;
                    return new Vector2(dpiScaleX / 1.5f, dpiScaleY / 1.5f);
                }
            }

            return Vector2.One;
        }

        private void CheckDpi()
        {
            UIController.GlobalScale = GetDpiScale();
        }

        private void Window_Move(Vector2D<int> newPos)
        {
            CheckDpi();
        }

        private void Window_FramebufferResize(Vector2D<int> newSize)
        {
            currentWindow = this;
            CheckDpi();
            Scene.ExecuteDeferredDelegates(DeferralMode.NextEvent);
            Scene.NotifyViewportChange(new Rectangle<int>(0, 0, newSize));
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
