using System.Windows;

namespace HandyScreenshot.Helpers
{
    public static class Constants
    {
        public static Rect RectZero { get; } = new Rect(0, 0, 0, 0);

        public static double ScaleFactor { get; } = MonitorHelper.GetScaleFactor();
    }
}
