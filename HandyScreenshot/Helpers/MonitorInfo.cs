using System;
using System.Diagnostics;
using System.Windows;

namespace HandyScreenshot.Helpers
{
    [DebuggerDisplay("[{PhysicalScreenRect}], [{PhysicalWorkArea}]")]
    public class MonitorInfo
    {
        public IntPtr Handle { get; }

        public bool IsPrimaryScreen { get; }

        public Rect PhysicalWorkArea { get; }

        public Rect PhysicalScreenRect { get; }

        public MonitorInfo(IntPtr handle, bool isPrimaryScreen, Rect workArea, Rect screenRect)
        {
            Handle = handle;
            IsPrimaryScreen = isPrimaryScreen;
            PhysicalWorkArea = workArea;
            PhysicalScreenRect = screenRect;
        }
    }
}
