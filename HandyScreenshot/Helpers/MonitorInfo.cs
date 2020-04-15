using System.Diagnostics;
using System.Windows;

namespace HandyScreenshot.Helpers
{
    [DebuggerDisplay("[{WorkArea}], [{ScreenRect}]")]
    public class MonitorInfo
    {
        public bool IsPrimaryScreen { get; }

        public Rect WorkArea { get; }

        public Rect ScreenRect { get; }

        public MonitorInfo(bool isPrimaryScreen, Rect workArea, Rect screenRect)
        {
            IsPrimaryScreen = isPrimaryScreen;
            WorkArea = workArea;
            ScreenRect = screenRect;
        }
    }
}
