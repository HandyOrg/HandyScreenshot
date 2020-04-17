using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
            var monitorInfos = MonitorHelper.GetMonitorInfos();
            var primaryScreen = monitorInfos.First(item => item.IsPrimaryScreen);
            var (primaryScreenScaleX, primaryScreenScaleY) = MonitorHelper.GetScaleFactorFromMonitor(primaryScreen.Handle);

            var detector = new RectDetector();
            detector.Snapshot(monitorInfos
                .Select(item => item.PhysicalScreenRect)
                .Aggregate((acc, item) =>
                {
                    var copy = new Rect(acc.X, acc.Y, acc.Width, acc.Height);
                    copy.Union(item);
                    return copy;
                }));

            foreach (var monitorInfo in monitorInfos)
            {
                var window = new MainWindow();

                var physicalRect = monitorInfo.PhysicalScreenRect;
                var rect = new Rect(physicalRect.X, physicalRect.Y, physicalRect.Width, physicalRect.Height);
                var (scaleX, scaleY) = MonitorHelper.GetScaleFactorFromMonitor(monitorInfo.Handle);
                rect.Scale(primaryScreenScaleX, primaryScreenScaleY);

                SetWindowRect(window, rect);

                var b = CaptureScreen(monitorInfo.PhysicalScreenRect);

                window.DataContext = new MainWindowViewModel
                {
                    MonitorInfo = monitorInfo,
                    ScaleX = scaleX,
                    ScaleY = scaleY,
                    Background = b,
                    Detector = detector
                };

                window.Loaded += WindowOnLoaded;

                window.Show();
            }
        }

        private static void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window window &&
                window.DataContext is MainWindowViewModel vm)
            {
                var source = PresentationSource.FromVisual(window);
                if (source?.CompositionTarget != null)
                {
                    var dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                    var dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
                    vm.DpiString = $"{dpiX}, {dpiY}";

                    //(vm.ScaleX, vm.ScaleY) = MonitorHelper.GetScaleFactorFromWindow(new WindowInteropHelper(window).EnsureHandle());
                }
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

            var image = Image.FromHbitmap(hBitmap);
            var bitmap = image.ToBitmapSource();
            //bitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty,
            //    BitmapSizeOptions.FromEmptyOptions());
            bitmap.Freeze();

            DeleteObject(hBitmap);
            DeleteDC(hdcDest);
            DeleteDC(hdcSrc);

            return bitmap;
        }

        public static BitmapSource ToBitmapSource(this Image image)
        {
            using var memoryStream = new MemoryStream();
            image.Save(memoryStream, ImageFormat.Jpeg);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public static IntPtr GetAllMonitorsDC()
        {
            return CreateDC("DISPLAY", null, null, IntPtr.Zero);
        }
    }
}
