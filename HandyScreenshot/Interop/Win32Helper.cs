using System.Windows;

namespace HandyScreenshot.Interop
{
    public static class Win32Helper
    {
        public static NativeMethods.POINT GetMousePosition()
        {
            var position = new NativeMethods.POINT();
            NativeMethods.GetCursorPos(ref position);
            return position;
        }

        public static Point ToPoint(this NativeMethods.POINT point, double scale = 1)
        {
            return new Point(point.X * scale, point.Y * scale);
        }

        public static Rect Scale(this Rect rect, double scale)
        {
            return new Rect(rect.Left * scale, rect.Top * scale, rect.Width * scale, rect.Height * scale);
        }
    }
}
