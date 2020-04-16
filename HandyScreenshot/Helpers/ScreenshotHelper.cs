using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using static HandyScreenshot.Interop.NativeMethods;

namespace HandyScreenshot.Helpers
{
    public static class ScreenshotHelper
    {
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
