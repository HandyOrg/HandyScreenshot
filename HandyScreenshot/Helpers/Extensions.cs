using System.Windows;
using HandyScreenshot.Common;
using static HandyScreenshot.Interop.NativeMethods;

namespace HandyScreenshot.Helpers
{
    public static class Extensions
    {
        public static Point ToPoint(this POINT point, double scaleX = 1D, double scaleY = 1D)
        {
            return new Point(point.X * scaleX, point.Y * scaleY);
        }

        public static Rect ToRect(this RECT rect, double scaleX = 1D, double scaleY = 1D)
        {
            return new Rect(
                rect.Left * scaleX,
                rect.Top * scaleY,
                (rect.Right - rect.Left) * scaleX,
                (rect.Bottom - rect.Top) * scaleY);
        }

        public static ReadOnlyRect ToReadOnlyRect(this RECT rect)
        {
            return (rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
        }
    }
}
