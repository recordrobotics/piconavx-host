using piconavx.ui.graphics;
using Silk.NET.Maths;
using WindowOptions = Silk.NET.Windowing.WindowOptions;

namespace piconavx.ui
{
    internal static class Program
    {
        private static HighLevelServer server;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            server = new HighLevelServer(65432);
            

            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1920, 1080);
            options.Position = new Vector2D<int>();
            options.Title = "piconavx ui";
            options.VSync = false;
            options.Samples = 8;
            options.IsVisible = false;
            options.WindowBorder = Silk.NET.Windowing.WindowBorder.Resizable;
            options.WindowState = Silk.NET.Windowing.WindowState.Maximized;

            Window window = new Window(options);
            window.Load += Window_Load;
            window.Run();
        }

        private static void Window_Load()
        {
            Window.Current.Internal.IsVisible = true; // only show window after loading graphics api
            Window.Current.Internal.WindowState = Silk.NET.Windowing.WindowState.Maximized;
            Scene.CreateTestScene();
        }
    }
}