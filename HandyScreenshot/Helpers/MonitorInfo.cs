using System.Diagnostics;
using System.Windows;

namespace HandyScreenshot.Helpers
{
    [DebuggerDisplay("[{PhysicalScreenRect}], [{PhysicalWorkArea}]")]
    public class MonitorInfo
    {
        public bool IsPrimaryScreen { get; }

        public Rect PhysicalWorkArea { get; }

        public Rect PhysicalScreenRect { get; }

        public MonitorInfo(bool isPrimaryScreen, Rect workArea, Rect screenRect)
        {
            IsPrimaryScreen = isPrimaryScreen;
            PhysicalWorkArea = workArea;
            PhysicalScreenRect = screenRect;
        }
    }
}
