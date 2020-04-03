using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static HandyScreenshot.Interop.MonitorNativeMethods;

namespace HandyScreenshot.Interop
{
    public static class MonitorHelper
    {
        private static readonly bool DpiApiLevel3 = Environment.OSVersion.Version >= new Version(6, 3);
        public const uint MonitorDefaultToNull = 0;

        public static readonly double ScaleFactor = GetScaleFactor();

        public static IEnumerable<MonitorInfo> GetMonitorInfos(double scale = 1D)
        {
            var monitors = new List<MONITORINFOEX>();
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr monitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr data) =>
                {
                    var info = new MONITORINFOEX
                    {
                        Size = Marshal.SizeOf(typeof(MONITORINFOEX))
                    };
                    GetMonitorInfo(monitor, ref info);
                    monitors.Add(info);
                    return true;
                },
                IntPtr.Zero);

            return monitors.Select(item => new MonitorInfo(
                item.Flags == 1,
                item.WorkArea.ToRect(scale),
                item.Monitor.ToRect(scale)));
        }

        public static double GetScaleFactor()
        {
            var window = new Window();
            var interop = new WindowInteropHelper(window);
            interop.EnsureHandle();
            var scale = GetScaleFactor(interop.Handle);
            window.Close();

            return scale;
        }

        public static double GetScaleFactor(IntPtr windowHandle)
        {
            if (DpiApiLevel3)
            {
                var hMonitor = MonitorFromWindow(windowHandle, MonitorDefaultToNull);
                var ret = GetDpiForMonitor(hMonitor, MonitorDpiType.MdtEffectiveDpi, out uint dpiX, out _);
                if (ret != 0)
                    throw new Win32Exception("Queries DPI of a display failed", Marshal.GetExceptionForHR(ret));

                return 96.0 / dpiX;
            }
            var windowDc = GetWindowDC(windowHandle);
            if (windowDc == IntPtr.Zero)
                throw new Win32Exception("Getting window device context failed");
            try
            {
                return 96.0 / GetDeviceCaps(windowDc, (int)DeviceCap.Logpixelsx);
            }
            finally
            {
                if (ReleaseDC(windowHandle, windowDc) == 0)
                    throw new Win32Exception("Releasing window device context failed");
            }
        }

        public static Rect ToRect(this RECT rect, double scale = 1D) => new Rect(
            rect.Left * scale,
            rect.Top * scale,
            (rect.Right - rect.Left) * scale,
            (rect.Bottom - rect.Top) * scale);
    }
}
