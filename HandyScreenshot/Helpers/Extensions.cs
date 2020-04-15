using System;
using System.Windows;
using System.Windows.Interop;
using static HandyScreenshot.Interop.NativeMethods;

namespace HandyScreenshot.Helpers
{
    public static class Extensions
    {
        public static Point ToPoint(this POINT point, double scale = 1D)
        {
            return new Point(point.X * scale, point.Y * scale);
        }

        public static Rect ToRect(this RECT rect, double scale = 1D)
        {
            return new Rect(
                rect.Left * scale,
                rect.Top * scale,
                (rect.Right - rect.Left) * scale,
                (rect.Bottom - rect.Top) * scale);
        }

        public static IntPtr GetHandle(this Window window)
        {
            return new WindowInteropHelper(window).EnsureHandle();
        }
    }
}
