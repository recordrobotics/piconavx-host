using piconavx.ui.graphics;
using Silk.NET.Maths;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Foundation;
using WindowOptions = Silk.NET.Windowing.WindowOptions;
using Windows.Win32.UI.Shell.Common;

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

            RECT rcWorkArea = new RECT();
            bool ok;
            unsafe
            {
                ok = PInvoke.SystemParametersInfo(SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETWORKAREA, 0, &rcWorkArea, 0);
                if (ok && OperatingSystem.IsWindowsVersionAtLeast(8, 1))
                {
                    HMONITOR monitor = PInvoke.MonitorFromPoint(new System.Drawing.Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);
                    if(PInvoke.GetScaleFactorForMonitor(monitor, out DEVICE_SCALE_FACTOR scaleFactor) == HRESULT.S_OK)
                    {
                        float scale = (int)scaleFactor / 100.0f;
                        rcWorkArea.left = (int)(rcWorkArea.left * scale);
                        rcWorkArea.top = (int)(rcWorkArea.top * scale);
                        rcWorkArea.right = (int)(rcWorkArea.right * scale);
                        rcWorkArea.bottom = (int)(rcWorkArea.bottom * scale);
                    }
                }
            }

            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "piconavx ui";
            options.VSync = false;
            options.Samples = 8;
            options.IsVisible = false;
            options.WindowBorder = Silk.NET.Windowing.WindowBorder.Resizable;
            options.WindowState = Silk.NET.Windowing.WindowState.Maximized;

            if (ok)
            {
                options.Position = new Vector2D<int>(rcWorkArea.Width / 2 - options.Size.X / 2, rcWorkArea.Height / 2 - options.Size.Y / 2);
            }

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