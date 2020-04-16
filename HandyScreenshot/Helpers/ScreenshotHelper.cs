using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using HandyScreenshot.Detection;
using static HandyScreenshot.Interop.NativeMethods;

namespace HandyScreenshot.Helpers
{
    public static class ScreenshotHelper
    {
        public static void StartScreenshot()
        {
            var monitorInfos = MonitorHelper.GetMonitorInfos().ToList();
            var primaryScreen = monitorInfos.First(item => item.IsPrimaryScreen);
            var (primaryScreenScaleX, primaryScreenScaleY) = MonitorHelper.GetScaleFactorFromMonitor(primaryScreen.Handle);


            var detector = new RectDetector();
            detector.Snapshot(monitorInfos
                .Select(item => item.PhysicalScreenRect)
                .Aggregate((acc, item) =>
                {
                    acc.Union(item);
                    return acc;
                }));
            foreach (var monitorInfo in monitorInfos)
            {
                var window = new MainWindow();

                var physicalRect = monitorInfo.PhysicalScreenRect;
                var rect = new Rect(physicalRect.X, physicalRect.Y, physicalRect.Width, physicalRect.Height);
                var (scaleX, scaleY) = MonitorHelper.GetScaleFactorFromMonitor(monitorInfo.Handle);
                rect.Scale(primaryScreenScaleX, primaryScreenScaleY);

                SetWindowRect(window, rect);

                window.DataContext = new MainWindowViewModel
                {
                    MonitorInfo = monitorInfo,
                    ScaleX = scaleX,
                    ScaleY = scaleY,
                    Background = CaptureScreen(monitorInfo.PhysicalScreenRect),
                    Detector = detector
                };

                window.Show();
            }
        }

        private static void SetWindowRect(Window window, Rect rect)
        {
            window.Left = rect.Left;
            window.Top = rect.Top;
            window.Width = rect.Width;
            window.Height = rect.Height;
        }

        public static BitmapSource CaptureScreen(Rect rect)
        {
            var hdcSrc = GetAllMonitorsDC();

            var width = (int)rect.Width;
            var height = (int)rect.Height;
            var hdcDest = CreateCompatibleDC(hdcSrc);
            var hBitmap = CreateCompatibleBitmap(hdcSrc, width, height);
            SelectObject(hdcDest, hBitmap);

            BitBlt(hdcDest, 0, 0, width, height, hdcSrc, (int)rect.X, (int)rect.Y,
                TernaryRasterOperations.SRCCOPY | TernaryRasterOperations.CAPTUREBLT);

            var bitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            bitmap.Freeze();

            DeleteObject(hBitmap);
            DeleteDC(hdcDest);
            DeleteDC(hdcSrc);

            return bitmap;
        }

        public static IntPtr GetAllMonitorsDC()
        {
            return CreateDC("DISPLAY", null, null, IntPtr.Zero);
        }
    }
}
