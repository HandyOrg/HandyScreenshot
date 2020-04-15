using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static HandyScreenshot.Interop.NativeMethods;

namespace HandyScreenshot.Helpers
{
    public static class MonitorHelper
    {
        private const double DefaultDpi = 96.0;
        private const uint MonitorDefaultToNull = 0;
        private static readonly bool DpiApiLevel3 = Environment.OSVersion.Version >= new Version(6, 3);

        public static IEnumerable<MonitorInfo> GetMonitorInfos()
        {
            var monitors = new List<MONITORINFOEX>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr monitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr data) =>
                {
                    var info = new MONITORINFOEX { Size = Marshal.SizeOf(typeof(MONITORINFOEX)) };
                    GetMonitorInfo(monitor, ref info);
                    monitors.Add(info);
                    return true;
                },
                IntPtr.Zero);

            return monitors.Select(item => new MonitorInfo(
                item.Flags == 1,
                item.WorkArea.ToRect(),
                item.Monitor.ToRect()));
        }

        public static (double scaleX, double scaleY) GetScaleFactor()
        {
            var window = new Window();
            var scale = GetScaleFactor(new WindowInteropHelper(window).EnsureHandle());
            window.Close();

            return scale;
        }

        public static (double scaleX, double scaleY) GetScaleFactor(IntPtr windowHandle)
        {
            if (DpiApiLevel3)
            {
                var hMonitor = MonitorFromWindow(windowHandle, MonitorDefaultToNull);
                var ret = GetDpiForMonitor(hMonitor, MonitorDpiType.MdtEffectiveDpi, out var dpiX, out var dpiY);
                if (ret != 0)
                    throw new Win32Exception("Queries DPI of a display failed", Marshal.GetExceptionForHR(ret));

                return (DefaultDpi / dpiX, DefaultDpi / dpiY);
            }
            var windowDc = GetWindowDC(windowHandle);
            if (windowDc == IntPtr.Zero)
                throw new Win32Exception("Getting window device context failed");

            try
            {
                var dpiX = GetDeviceCaps(windowDc, DeviceCap.Logpixelsx);
                var dpiY = GetDeviceCaps(windowDc, DeviceCap.Logpixelsy);
                return (DefaultDpi / dpiX, DefaultDpi / dpiY);
            }
            finally
            {
                if (ReleaseDC(windowHandle, windowDc) == 0)
                    throw new Win32Exception("Releasing window device context failed");
            }
        }
    }
}
