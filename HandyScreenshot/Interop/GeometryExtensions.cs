using System;
using System.Windows;

namespace HandyScreenshot
{
    public static class GeometryExtensions
    {
        public static Point GetCentralPoint(this Rect rect)
        {
            return new Point((rect.Right + rect.Left) / 2, (rect.Bottom + rect.Top) / 2);
        }

        public static double GetDistance(this Point point1, Point point2)
        {
            return Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2);
        }

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
