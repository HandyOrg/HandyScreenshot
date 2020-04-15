using System.Windows;
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

        public static Rect Scale(this Rect rect, double scale)
        {
            return new Rect(
                rect.Left * scale, 
                rect.Top * scale, 
                rect.Width * scale, 
                rect.Height * scale);
        }
    }
}
