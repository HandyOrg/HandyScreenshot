using System;
using System.Diagnostics;
using System.Windows;
using HandyScreenshot.Common;

namespace HandyScreenshot.Helpers
{
    [DebuggerDisplay("[{PhysicalScreenRect}], [{PhysicalWorkArea}]")]
    public class MonitorInfo
    {
        public IntPtr Handle { get; }

        public bool IsPrimaryScreen { get; }

        public ReadOnlyRect PhysicalWorkArea { get; }

        public ReadOnlyRect PhysicalScreenRect { get; }

        public MonitorInfo(IntPtr handle, bool isPrimaryScreen, ReadOnlyRect workArea, ReadOnlyRect screenRect)
        {
            Handle = handle;
            IsPrimaryScreen = isPrimaryScreen;
            PhysicalWorkArea = workArea;
            PhysicalScreenRect = screenRect;
        }
    }
}
