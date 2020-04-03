using System.Windows;

namespace HandyScreenshot.Interop
{
    public static class GeometryExtensions
    {
        public static Point ToPoint(this NativeMethods.POINT point, double scale = 1)
        {
            return new Point(point.X * scale, point.Y * scale);
        }

        public static Rect ToRect(this NativeMethods.RECT rect, double scale = 1)
        {
            return new Rect(
                rect.Left * scale,
                rect.Top * scale,
                (rect.Right - rect.Left) * scale,
                (rect.Bottom - rect.Top) * scale);
        }
    }
}
